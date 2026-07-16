import math
import os
import sys

import bpy
from mathutils import Vector


PROJECT_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "../.."))
CHARACTER_DIR = os.path.join(PROJECT_ROOT, "Assets/Resources/OriginalArt/Characters")
TEXTURE_DIR = os.path.join(PROJECT_ROOT, "Assets/Resources/OriginalArt/Textures")
SOURCE_DIR = os.path.join(PROJECT_ROOT, "Assets/Art/Original")
ATLAS_SIZE = 2048
TILE_SIZE = 256
TILES_PER_SIDE = ATLAS_SIZE // TILE_SIZE

PALETTES = {
    "Guerrero": {
        "primary": (0.12, 0.32, 0.75, 1.0),
        "metal": (0.62, 0.68, 0.76, 1.0),
        "fabric": (0.06, 0.10, 0.18, 1.0),
        "glow": (1.0, 0.63, 0.12, 1.0),
    },
    "Ninja": {
        "primary": (0.08, 0.10, 0.17, 1.0),
        "metal": (0.42, 0.52, 0.68, 1.0),
        "fabric": (0.03, 0.04, 0.08, 1.0),
        "glow": (0.36, 0.76, 1.0, 1.0),
    },
    "Chaman": {
        "primary": (0.18, 0.55, 0.82, 1.0),
        "metal": (0.78, 0.62, 0.30, 1.0),
        "fabric": (0.12, 0.25, 0.32, 1.0),
        "glow": (0.22, 0.95, 0.84, 1.0),
    },
    "Umbra": {
        "primary": (0.28, 0.10, 0.44, 1.0),
        "metal": (0.34, 0.20, 0.50, 1.0),
        "fabric": (0.07, 0.025, 0.12, 1.0),
        "glow": (0.75, 0.25, 1.0, 1.0),
    },
}

PATTERN_NAMES = ("metal", "fabric", "leather", "rune", "skin", "bone", "scale", "stone")
ATLAS_ENTRIES = {}
MATERIAL_CACHE = {}
ALBEDO_PIXELS = {}
NORMAL_PIXELS = {}
ALBEDO_IMAGES = {}
NORMAL_IMAGES = {}


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.armatures, bpy.data.cameras, bpy.data.lights):
        for datablock in list(datablocks):
            if datablock.users == 0:
                datablocks.remove(datablock)
    for action in list(bpy.data.actions):
        bpy.data.actions.remove(action)


def rgba(value):
    return (value[0], value[1], value[2], 1.0)


def clamp(value):
    return max(0.0, min(1.0, value))


def atlas_key(class_name, pattern):
    return (class_name, pattern)


def build_atlas_pixels():
    global ALBEDO_PIXELS, NORMAL_PIXELS
    ATLAS_ENTRIES.clear()
    for class_name, palette in PALETTES.items():
        pixel_count = ATLAS_SIZE * ATLAS_SIZE * 4
        albedo_pixels = [0.0] * pixel_count
        normal_pixels = [0.5, 0.5, 1.0, 1.0] * (ATLAS_SIZE * ATLAS_SIZE)
        slot = 0
        for pattern in PATTERN_NAMES:
            tile_x = (slot % TILES_PER_SIDE) * TILE_SIZE
            tile_y = (slot // TILES_PER_SIDE) * TILE_SIZE
            ATLAS_ENTRIES[atlas_key(class_name, pattern)] = (
                tile_x / ATLAS_SIZE,
                tile_y / ATLAS_SIZE,
                TILE_SIZE / ATLAS_SIZE,
                TILE_SIZE / ATLAS_SIZE,
            )
            color = pattern_color(palette, pattern)
            for y in range(TILE_SIZE):
                for x in range(TILE_SIZE):
                    height = pattern_height(pattern, x, y)
                    wave = (height - 0.5) * 0.18
                    pixel = [clamp(channel * (0.90 + wave)) for channel in color[:3]]
                    if height > 0.58:
                        pixel = blend(pixel, (1.0, 1.0, 1.0), min((height - 0.58) * 0.18, 0.10))
                    elif height < 0.42:
                        pixel = blend(pixel, (0.0, 0.0, 0.0), min((0.42 - height) * 0.22, 0.12))

                    normal_x, normal_y = baked_normal(pattern, x, y)

                    write_pixel(albedo_pixels, tile_x + x, tile_y + y, pixel + [1.0])
                    write_pixel(normal_pixels, tile_x + x, tile_y + y, [normal_x, normal_y, 1.0, 1.0])
            slot += 1
        ALBEDO_PIXELS[class_name] = albedo_pixels
        NORMAL_PIXELS[class_name] = normal_pixels


def pattern_height(pattern, x, y):
    u = (x + 0.5) / TILE_SIZE
    v = (y + 0.5) / TILE_SIZE
    if pattern == "metal":
        ridge = math.sin(u * math.pi * 12.0) * 0.08
        groove = math.cos(v * math.pi * 5.0) * 0.035
        return 0.5 + ridge + groove
    if pattern == "fabric":
        return 0.5 + math.sin(u * math.pi * 32.0) * 0.045 + math.sin(v * math.pi * 28.0) * 0.04
    if pattern == "leather":
        grain = math.sin((u * 17.0 + v * 9.0) * math.pi) * math.cos((v * 23.0 - u * 5.0) * math.pi)
        return 0.5 + grain * 0.075
    if pattern == "rune":
        diagonal = min(abs(u - v), abs((1.0 - u) - v))
        return 0.66 if diagonal < 0.035 else 0.5
    if pattern == "scale":
        cell_u = (u * 8.0) % 1.0
        cell_v = (v * 8.0) % 1.0
        distance = math.sqrt((cell_u - 0.5) ** 2 + (cell_v - 0.42) ** 2)
        return 0.64 if distance < 0.32 else 0.46
    if pattern == "stone":
        crack = math.sin((u * 7.0 + v * 11.0) * math.pi) * 0.5 + 0.5
        return 0.43 + crack * 0.14
    if pattern == "bone":
        return 0.5 + math.sin((u * 4.0 + v * 2.0) * math.pi) * 0.07
    return 0.5


def baked_normal(pattern, x, y):
    left = pattern_height(pattern, (x - 1) % TILE_SIZE, y)
    right = pattern_height(pattern, (x + 1) % TILE_SIZE, y)
    down = pattern_height(pattern, x, (y - 1) % TILE_SIZE)
    up = pattern_height(pattern, x, (y + 1) % TILE_SIZE)
    normal = Vector((-(right - left) * 2.8, -(up - down) * 2.8, 1.0)).normalized()
    return clamp(normal.x * 0.5 + 0.5), clamp(normal.y * 0.5 + 0.5)


def pattern_color(palette, pattern):
    if pattern == "metal":
        return palette["metal"]
    if pattern in ("fabric", "leather"):
        return palette["fabric"]
    if pattern == "rune":
        return palette["glow"]
    if pattern == "skin":
        return (0.78, 0.50, 0.34, 1.0)
    if pattern == "bone":
        return (0.62, 0.42, 0.20, 1.0)
    if pattern == "scale":
        return palette["primary"]
    return (0.24, 0.25, 0.28, 1.0)


def blend(first, second, amount):
    return [first[index] * (1.0 - amount) + second[index] * amount for index in range(3)]


def write_pixel(pixels, x, y, value):
    index = (y * ATLAS_SIZE + x) * 4
    pixels[index:index + 4] = value


def save_atlas_images(albedo_images=None, normal_images=None, write_pixels=True):
    global ALBEDO_IMAGES, NORMAL_IMAGES
    os.makedirs(TEXTURE_DIR, exist_ok=True)
    albedo_images = albedo_images or {}
    normal_images = normal_images or {}
    for class_name in PALETTES:
        albedo_name = f"OriginalArt_{class_name}_AlbedoAtlas_2K"
        normal_name = f"OriginalArt_{class_name}_NormalAtlas_2K"
        albedo = albedo_images.get(class_name) or bpy.data.images.get(albedo_name)
        normal = normal_images.get(class_name) or bpy.data.images.get(normal_name)
        if albedo is not None and (albedo.size[0] != ATLAS_SIZE or albedo.size[1] != ATLAS_SIZE):
            bpy.data.images.remove(albedo)
            albedo = None
        if normal is not None and (normal.size[0] != ATLAS_SIZE or normal.size[1] != ATLAS_SIZE):
            bpy.data.images.remove(normal)
            normal = None
        albedo = albedo or bpy.data.images.new(albedo_name, width=ATLAS_SIZE, height=ATLAS_SIZE, alpha=True)
        normal = normal or bpy.data.images.new(normal_name, width=ATLAS_SIZE, height=ATLAS_SIZE, alpha=True)
        if write_pixels:
            albedo.pixels = ALBEDO_PIXELS[class_name]
            normal.pixels = NORMAL_PIXELS[class_name]
        albedo.filepath_raw = os.path.join(TEXTURE_DIR, f"OriginalArt_{class_name}_AlbedoAtlas_2K.png")
        normal.filepath_raw = os.path.join(TEXTURE_DIR, f"OriginalArt_{class_name}_NormalAtlas_2K.png")
        albedo.file_format = "PNG"
        normal.file_format = "PNG"
        normal.colorspace_settings.name = "Non-Color"
        albedo.save()
        normal.save()
        albedo_images[class_name] = albedo
        normal_images[class_name] = normal
    ALBEDO_IMAGES = albedo_images
    NORMAL_IMAGES = normal_images
    return albedo_images, normal_images


def create_material(class_name, pattern, albedo, normal):
    key = atlas_key(class_name, pattern)
    if key in MATERIAL_CACHE:
        return MATERIAL_CACHE[key]

    material = bpy.data.materials.new(f"Original_{class_name}_{pattern}")
    material.use_nodes = True
    nodes = material.node_tree.nodes
    links = material.node_tree.links
    bsdf = nodes.get("Principled BSDF")
    entry = ATLAS_ENTRIES[key]
    scale = (entry[2], entry[3])
    offset = (entry[0], entry[1])

    texture = nodes.new("ShaderNodeTexImage")
    texture.image = albedo
    texture.extension = "CLIP"
    texture.interpolation = "Linear"
    texture.projection = "FLAT"
    mapping = nodes.new("ShaderNodeMapping")
    mapping.inputs["Scale"].default_value = (scale[0], scale[1], 1.0)
    mapping.inputs["Location"].default_value = (offset[0], offset[1], 0.0)
    uv = nodes.new("ShaderNodeTexCoord")
    links.new(uv.outputs["UV"], mapping.inputs["Vector"])
    links.new(mapping.outputs["Vector"], texture.inputs["Vector"])
    links.new(texture.outputs["Color"], bsdf.inputs["Base Color"])

    normal_texture = nodes.new("ShaderNodeTexImage")
    normal_texture.image = normal
    normal_texture.image.colorspace_settings.name = "Non-Color"
    normal_texture.extension = "CLIP"
    normal_mapping = nodes.new("ShaderNodeMapping")
    normal_mapping.inputs["Scale"].default_value = (scale[0], scale[1], 1.0)
    normal_mapping.inputs["Location"].default_value = (offset[0], offset[1], 0.0)
    normal_map = nodes.new("ShaderNodeNormalMap")
    links.new(uv.outputs["UV"], normal_mapping.inputs["Vector"])
    links.new(normal_mapping.outputs["Vector"], normal_texture.inputs["Vector"])
    links.new(normal_texture.outputs["Color"], normal_map.inputs["Color"])
    links.new(normal_map.outputs["Normal"], bsdf.inputs["Normal"])

    bsdf.inputs["Metallic"].default_value = 0.7 if pattern == "metal" else 0.16
    bsdf.inputs["Roughness"].default_value = 0.32 if pattern == "metal" else 0.55
    MATERIAL_CACHE[key] = material
    return material


def make_armature():
    armature_data = bpy.data.armatures.new("OriginalCharacterRig")
    armature = bpy.data.objects.new("OriginalCharacterRig", armature_data)
    bpy.context.collection.objects.link(armature)
    bpy.context.view_layer.objects.active = armature
    armature.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")

    bones = {}

    def bone(name, head, tail, parent=None):
        item = armature_data.edit_bones.new(name)
        item.head = head
        item.tail = tail
        if parent:
            item.parent = bones[parent]
            item.use_connect = False
        bones[name] = item

    bone("Root", (0, 0, 0), (0, 0.2, 0))
    bone("Hips", (0, -0.28, 0), (0, 0.02, 0), "Root")
    bone("Spine", (0, 0.02, 0), (0, 0.30, 0), "Hips")
    bone("Chest", (0, 0.30, 0), (0, 0.52, 0), "Spine")
    bone("Neck", (0, 0.52, 0), (0, 0.68, 0), "Chest")
    bone("Head", (0, 0.68, 0), (0, 1.08, 0), "Neck")
    bone("Shoulder.L", (-0.42, 0.40, 0), (-0.55, 0.40, 0), "Chest")
    bone("UpperArm.L", (-0.55, 0.40, 0), (-0.55, 0.08, 0), "Shoulder.L")
    bone("LowerArm.L", (-0.55, 0.08, 0), (-0.55, -0.24, 0), "UpperArm.L")
    bone("Hand.L", (-0.55, -0.24, 0), (-0.55, -0.42, 0), "LowerArm.L")
    bone("Shoulder.R", (0.42, 0.40, 0), (0.55, 0.40, 0), "Chest")
    bone("UpperArm.R", (0.55, 0.40, 0), (0.55, 0.08, 0), "Shoulder.R")
    bone("LowerArm.R", (0.55, 0.08, 0), (0.55, -0.24, 0), "UpperArm.R")
    bone("Hand.R", (0.55, -0.24, 0), (0.55, -0.42, 0), "LowerArm.R")
    bone("Thigh.L", (-0.20, -0.30, 0), (-0.20, -0.70, 0), "Hips")
    bone("Foot.L", (-0.20, -0.70, 0), (-0.20, -0.98, -0.12), "Thigh.L")
    bone("Thigh.R", (0.20, -0.30, 0), (0.20, -0.70, 0), "Hips")
    bone("Foot.R", (0.20, -0.70, 0), (0.20, -0.98, -0.12), "Thigh.R")
    bpy.ops.object.mode_set(mode="POSE")
    for pose_bone in armature.pose.bones:
        pose_bone.rotation_mode = "XYZ"
    bpy.ops.object.mode_set(mode="OBJECT")
    armature.display_type = "WIRE"
    return armature


def create_part(name, primitive, location, scale, material, bone_name, rotation=(0, 0, 0), vertices=8):
    if primitive == "sphere":
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=4, radius=1.0, location=location)
    elif primitive == "cone":
        bpy.ops.mesh.primitive_cone_add(vertices=max(vertices, 18), radius1=1.0, radius2=0.7, depth=1.0, location=location)
    else:
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    obj.rotation_euler = rotation
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    if primitive == "cube":
        bevel = obj.modifiers.new("Soft Armor Edges", "BEVEL")
        bevel.width = min(min(obj.dimensions) * 0.22, 0.055)
        bevel.segments = 3
        bevel.limit_method = "ANGLE"
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_apply(modifier=bevel.name)
        weighted = obj.modifiers.new("Weighted Armor Normals", "WEIGHTED_NORMAL")
        weighted.keep_sharp = True
        weighted.weight = 50
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_apply(modifier=weighted.name)
    if primitive in ("sphere", "cone"):
        for polygon in obj.data.polygons:
            polygon.use_smooth = True
    obj.data.materials.append(material)
    assign_uvs(obj)
    group = obj.vertex_groups.new(name=bone_name)
    group.add(list(range(len(obj.data.vertices))), 1.0, "REPLACE")
    return obj


def assign_uvs(obj):
    mesh = obj.data
    uv_layer = mesh.uv_layers.new(name="UVMap")
    for loop in mesh.loops:
        coordinate = mesh.vertices[loop.vertex_index].co
        uv_layer.data[loop.index].uv = (0.5 + coordinate.x * 0.45, 0.5 + coordinate.z * 0.45)


def join_parts(parts, armature, name):
    bpy.ops.object.select_all(action="DESELECT")
    for part in parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    mesh = bpy.context.object
    mesh.name = name
    modifier = mesh.modifiers.new("Original Rig Skin", "ARMATURE")
    modifier.object = armature
    mesh.parent = armature
    mesh["original_art"] = True
    mesh["rig_version"] = "1.0"
    return mesh


def pattern_from_material(material):
    if material is None:
        return "stone"
    name = material.name.lower()
    for pattern in PATTERN_NAMES:
        if pattern in name:
            return pattern
    return "stone"


def create_highpoly_source(mesh, name):
    highpoly = mesh.copy()
    highpoly.data = mesh.data.copy()
    highpoly.name = name
    bpy.context.collection.objects.link(highpoly)
    highpoly.parent = None
    highpoly.matrix_world = mesh.matrix_world.copy()
    for modifier in list(highpoly.modifiers):
        highpoly.modifiers.remove(modifier)

    subdiv = highpoly.modifiers.new("High Poly Surface", "SUBSURF")
    subdiv.subdivision_type = "SIMPLE"
    subdiv.levels = 1
    subdiv.render_levels = 1
    bpy.context.view_layer.objects.active = highpoly
    highpoly.select_set(True)
    bpy.ops.object.modifier_apply(modifier=subdiv.name)

    detail_texture = bpy.data.textures.new(f"{name} Sculpt Detail", type="CLOUDS")
    detail_texture.noise_scale = 0.12
    detail_texture.noise_depth = 2
    displace = highpoly.modifiers.new("Sculpt Surface Detail", "DISPLACE")
    displace.texture = detail_texture
    displace.texture_coords = "GLOBAL"
    displace.direction = "NORMAL"
    displace.strength = 0.008
    displace.mid_level = 0.5
    bpy.context.view_layer.objects.active = highpoly
    bpy.ops.object.modifier_apply(modifier=displace.name)
    highpoly.select_set(False)
    source_material = bpy.data.materials.new(f"{name} Neutral Source")
    highpoly.data.materials.clear()
    highpoly.data.materials.append(source_material)
    for polygon in highpoly.data.polygons:
        polygon.material_index = 0
    return highpoly, detail_texture, source_material


def remap_uvs_to_atlas(mesh, class_name):
    uv_layer = mesh.data.uv_layers.get("UVMap")
    if uv_layer is None:
        raise RuntimeError(f"Missing UVMap on {mesh.name}")
    for polygon in mesh.data.polygons:
        pattern = pattern_from_material(mesh.data.materials[polygon.material_index])
        entry = ATLAS_ENTRIES[(class_name, pattern)]
        for loop_index in polygon.loop_indices:
            uv = uv_layer.data[loop_index].uv.copy()
            uv_layer.data[loop_index].uv = (
                entry[0] + uv.x * entry[2],
                entry[1] + uv.y * entry[3],
            )


def bake_normal_atlas(mesh, class_name, normal_image):
    highpoly, detail_texture, source_material = create_highpoly_source(mesh, f"{mesh.name}_HighPolyBake")
    target = mesh.copy()
    target.data = mesh.data.copy()
    target.name = f"{mesh.name}_NormalBakeTarget"
    bpy.context.collection.objects.link(target)
    target.parent = None
    target.matrix_world = mesh.matrix_world.copy()
    for modifier in list(target.modifiers):
        target.modifiers.remove(modifier)

    bake_material = bpy.data.materials.new(f"{mesh.name}_NormalBakeMaterial")
    bake_material.use_nodes = True
    nodes = bake_material.node_tree.nodes
    nodes.clear()
    output = nodes.new("ShaderNodeOutputMaterial")
    shader = nodes.new("ShaderNodeBsdfPrincipled")
    image_texture = nodes.new("ShaderNodeTexImage")
    image_texture.image = normal_image
    image_texture.select = True
    nodes.active = image_texture
    bake_material.node_tree.links.new(shader.outputs["BSDF"], output.inputs["Surface"])
    target.data.materials.clear()
    target.data.materials.append(bake_material)
    for polygon in target.data.polygons:
        polygon.material_index = 0
    remap_uvs_to_atlas(target, class_name)

    scene = bpy.context.scene
    previous_engine = scene.render.engine
    bpy.ops.object.select_all(action="DESELECT")
    highpoly.select_set(True)
    target.select_set(True)
    bpy.context.view_layer.objects.active = target
    try:
        scene.render.engine = "CYCLES"
        scene.cycles.samples = 4
        bpy.ops.object.bake(type="NORMAL", normal_space="TANGENT", margin=8, use_clear=False)
        normal_image.update()
        normal_image.save()
        print(f"BAKED_NORMALS {class_name} {mesh.name}")
    except Exception as error:
        print(f"BAKE_NORMALS_FALLBACK {class_name} {mesh.name}: {error}")
    finally:
        scene.render.engine = previous_engine
        bpy.data.objects.remove(highpoly, do_unlink=True)
        bpy.data.objects.remove(target, do_unlink=True)
        bpy.data.materials.remove(bake_material)
        bpy.data.materials.remove(source_material)
        bpy.data.textures.remove(detail_texture)


def create_lod_mesh(source, armature, name, ratio):
    lod = source.copy()
    lod.data = source.data.copy()
    lod.name = name
    bpy.context.collection.objects.link(lod)
    lod.parent = armature
    lod["original_art"] = True
    lod["lod_level"] = int(name.rsplit("LOD", 1)[1])

    for modifier in list(lod.modifiers):
        lod.modifiers.remove(modifier)
    armature_modifier = lod.modifiers.new("Original Rig Skin", "ARMATURE")
    armature_modifier.object = armature
    decimate = lod.modifiers.new("Geometry LOD", "DECIMATE")
    decimate.ratio = ratio
    decimate.use_collapse_triangulate = True

    bpy.ops.object.select_all(action="DESELECT")
    lod.select_set(True)
    bpy.context.view_layer.objects.active = lod
    while lod.modifiers.find(decimate.name) > 0:
        bpy.ops.object.modifier_move_up(modifier=decimate.name)
    bpy.ops.object.modifier_apply(modifier=decimate.name)
    return lod


def create_action(armature, name, mode):
    action = bpy.data.actions.new(name)
    action.use_fake_user = True
    armature.animation_data_create()
    slot = action.slots.new("OBJECT", armature.name)
    layer = action.layers.new("Base")
    strip = layer.strips.new(type="KEYFRAME")
    channelbag = strip.channelbag(slot, ensure=True)
    if mode == "idle":
        frames = (1, 20, 40, 60, 80)
        phases = (0.0, 1.0, 0.0, -1.0, 0.0)
        targets = {
            "Hips": (0.0, 0.0, math.radians(1.5)),
            "Spine": (0.0, 0.0, math.radians(2.4)),
            "Chest": (math.radians(1.5), 0.0, math.radians(-1.8)),
            "Neck": (0.0, 0.0, math.radians(-1.2)),
            "Head": (0.0, 0.0, math.radians(1.8)),
            "Shoulder.L": (0.0, 0.0, math.radians(-2.0)),
            "Shoulder.R": (0.0, 0.0, math.radians(2.0)),
            "UpperArm.L": (0.0, math.radians(4), math.radians(1.5)),
            "UpperArm.R": (0.0, math.radians(-4), math.radians(-1.5)),
            "LowerArm.L": (math.radians(2), 0.0, 0.0),
            "LowerArm.R": (math.radians(-2), 0.0, 0.0),
        }
    elif mode == "run":
        frames = (1, 6, 11, 16, 21)
        phases = (0.0, 1.0, 0.0, -1.0, 0.0)
        targets = {
            "Hips": (0.0, 0.0, math.radians(4.0)),
            "Spine": (math.radians(-3.0), 0.0, math.radians(-3.0)),
            "Chest": (math.radians(4.0), 0.0, math.radians(2.5)),
            "Neck": (math.radians(1.5), 0.0, math.radians(2.0)),
            "Head": (math.radians(-2.0), 0.0, math.radians(-1.5)),
            "Shoulder.L": (0.0, 0.0, math.radians(4.0)),
            "Shoulder.R": (0.0, 0.0, math.radians(-4.0)),
            "UpperArm.L": (math.radians(-30), 0.0, math.radians(10)),
            "UpperArm.R": (math.radians(30), 0.0, math.radians(-10)),
            "LowerArm.L": (math.radians(-12), 0.0, math.radians(3)),
            "LowerArm.R": (math.radians(12), 0.0, math.radians(-3)),
            "Hand.L": (math.radians(-8), 0.0, 0.0),
            "Hand.R": (math.radians(8), 0.0, 0.0),
            "Thigh.L": (math.radians(34), 0.0, 0.0),
            "Thigh.R": (math.radians(-34), 0.0, 0.0),
            "Foot.L": (math.radians(-18), 0.0, 0.0),
            "Foot.R": (math.radians(18), 0.0, 0.0),
        }
    else:
        frames = (1, 5, 10, 16, 24, 32)
        phases = (0.0, 0.35, 1.0, 0.55, 0.15, 0.0)
        targets = {
            "Hips": (math.radians(-4), 0.0, math.radians(-5)),
            "Spine": (math.radians(10), 0.0, math.radians(-12)),
            "Chest": (math.radians(14), 0.0, math.radians(-18)),
            "Neck": (math.radians(-12), 0.0, math.radians(10)),
            "Head": (math.radians(-8), 0.0, math.radians(8)),
            "Shoulder.L": (math.radians(-10), 0.0, math.radians(-8)),
            "Shoulder.R": (math.radians(18), 0.0, math.radians(12)),
            "UpperArm.L": (math.radians(-54), 0.0, math.radians(18)),
            "UpperArm.R": (math.radians(72), 0.0, math.radians(-26)),
            "LowerArm.L": (math.radians(-28), 0.0, math.radians(8)),
            "LowerArm.R": (math.radians(42), 0.0, math.radians(-12)),
            "Hand.L": (math.radians(-20), 0.0, math.radians(6)),
            "Hand.R": (math.radians(28), 0.0, math.radians(-8)),
            "Thigh.L": (math.radians(-12), 0.0, 0.0),
            "Thigh.R": (math.radians(16), 0.0, 0.0),
            "Foot.L": (math.radians(8), 0.0, 0.0),
            "Foot.R": (math.radians(-8), 0.0, 0.0),
        }

    for bone_name, target in targets.items():
        for axis in range(3):
            curve = channelbag.fcurves.new(f'pose.bones["{bone_name}"].rotation_euler', index=axis, group_name=bone_name)
            for index, frame in enumerate(frames):
                value = target[axis] * phases[index]
                curve.keyframe_points.insert(frame, value)
            for point in curve.keyframe_points:
                point.interpolation = "BEZIER"

    armature.animation_data.action = action
    armature.animation_data.action_slot = slot
    return action


def build_character(class_name, gender):
    palette = PALETTES[class_name]
    female = gender == "Femenino"
    albedo = ALBEDO_IMAGES[class_name]
    normal = NORMAL_IMAGES[class_name]
    armor = create_material(class_name, "metal", albedo, normal)
    fabric = create_material(class_name, "fabric", albedo, normal)
    leather = create_material(class_name, "leather", albedo, normal)
    rune = create_material(class_name, "rune", albedo, normal)
    skin = create_material(class_name, "skin", albedo, normal)
    parts = []
    armature = make_armature()
    width = 0.42 if female else 0.50
    hip = 0.24 if female else 0.20

    parts.append(create_part("Torso", "cone", (0, 0.04, 0), (0.42, 0.40, 0.78), armor if class_name != "Ninja" else fabric, "Spine"))
    parts.append(create_part("Head", "sphere", (0, 0.84, 0), (0.34, 0.34, 0.34), skin, "Head"))
    parts.append(create_part("Left Arm", "cone", (-width, 0.06, 0), (0.17, 0.17, 0.40), leather, "UpperArm.L", (0, 0, math.radians(16))))
    parts.append(create_part("Right Arm", "cone", (width, 0.06, 0), (0.17, 0.17, 0.40), leather, "UpperArm.R", (0, 0, math.radians(-16))))
    parts.append(create_part("Left Leg", "cone", (-hip, -0.58, 0), (0.20, 0.20, 0.56), leather, "Thigh.L"))
    parts.append(create_part("Right Leg", "cone", (hip, -0.58, 0), (0.20, 0.20, 0.56), leather, "Thigh.R"))
    parts.append(create_part("Left Boot", "cone", (-hip, -0.91, -0.08), (0.23, 0.23, 0.22), armor, "Foot.L"))
    parts.append(create_part("Right Boot", "cone", (hip, -0.91, -0.08), (0.23, 0.23, 0.22), armor, "Foot.R"))

    if class_name == "Guerrero":
        parts += build_warrior(class_name, gender, armor, fabric, rune, female)
    elif class_name == "Ninja":
        parts += build_ninja(class_name, gender, armor, fabric, rune, female)
    elif class_name == "Chaman":
        parts += build_chaman(class_name, gender, armor, fabric, rune, female)
    else:
        parts += build_umbra(class_name, gender, armor, fabric, rune, female)

    # Keep the starter weapon as an independent skinned object. Unity can then
    # hide it when a real equipment item is equipped without touching the body.
    weapon_keywords = ("Sword", "Dagger", "Staff", "Blade")
    weapon_parts = [part for part in parts if any(keyword in part.name for keyword in weapon_keywords)]
    body_parts = [part for part in parts if part not in weapon_parts]
    mesh = join_parts(body_parts, armature, f"Original_{class_name}_{gender}_LOD0")
    weapon_mesh = None
    if weapon_parts:
        weapon_mesh = join_parts(weapon_parts, armature, "Starter Weapon LOD0")
    bake_normal_atlas(mesh, class_name, normal)
    if weapon_mesh is not None:
        bake_normal_atlas(weapon_mesh, class_name, normal)
    lod_meshes = []
    for level, ratio in ((1, 0.55), (2, 0.28)):
        lod_meshes.append(create_lod_mesh(mesh, armature, f"Original_{class_name}_{gender}_LOD{level}", ratio))
        if weapon_mesh is not None:
            lod_meshes.append(create_lod_mesh(weapon_mesh, armature, f"Starter Weapon LOD{level}", ratio))
    for mode in ("idle", "run", "attack"):
        create_action(armature, f"Original_{class_name}_{gender}_{mode.title()}", mode)

    armature["class"] = class_name
    armature["gender"] = gender
    mesh["class"] = class_name
    mesh["gender"] = gender
    export_character(class_name, gender, mesh, armature, weapon_mesh, lod_meshes)


def build_warrior(class_name, gender, armor, fabric, rune, female):
    parts = [
        create_part("Shoulder.L", "sphere", (-0.50, 0.42, 0), (0.30, 0.20, 0.28), armor, "Chest"),
        create_part("Shoulder.R", "sphere", (0.50, 0.42, 0), (0.30, 0.20, 0.28), armor, "Chest"),
        create_part("Chest Rune", "cone", (0, 0.18, -0.40), (0.12, 0.08, 0.08), rune, "Chest", (math.radians(90), 0, 0), 4),
        create_part("Cape", "cube", (0, 0.04, 0.34), (0.72, 0.76, 0.06), fabric, "Spine", (math.radians(8), 0, 0)),
    ]
    if female:
        parts.append(create_part("Tiara", "cone", (0, 1.16, -0.02), (0.30, 0.08, 0.22), rune, "Head", vertices=6))
    else:
        parts.append(create_part("Helmet Crest", "cone", (0, 1.28, -0.02), (0.12, 0.26, 0.12), rune, "Head", (0, 0, math.radians(45)), vertices=4))
    parts += build_sword(armor, rune, ALBEDO_IMAGES[class_name], NORMAL_IMAGES[class_name])
    return parts


def build_ninja(class_name, gender, armor, fabric, rune, female):
    parts = [
        create_part("Hood", "sphere", (0, 0.96, 0.02), (0.40, 0.36, 0.40), fabric, "Head"),
        create_part("Mask", "cube", (0, 0.86, -0.34), (0.28, 0.08, 0.05), rune, "Head"),
        create_part("Sash", "cube", (0, -0.02, -0.42), (0.52, 0.07, 0.05), armor, "Hips"),
    ]
    if female:
        parts.append(create_part("Hair Ribbon", "cone", (0.26, 0.64, 0), (0.05, 0.05, 0.26), rune, "Head", (0, 0, math.radians(-16)), vertices=5))
    parts += [
        create_part("Dagger.L", "cone", (-0.48, 0.02, -0.28), (0.07, 0.07, 0.30), armor, "Hand.L", (0, 0, math.radians(26)), vertices=4),
        create_part("Dagger.R", "cone", (0.48, 0.02, -0.28), (0.07, 0.07, 0.30), armor, "Hand.R", (0, 0, math.radians(-26)), vertices=4),
    ]
    return parts


def build_chaman(class_name, gender, armor, fabric, rune, female):
    parts = [
        create_part("Robe", "cone", (0, -0.48, 0), (0.52, 0.34, 0.46), fabric, "Hips"),
        create_part("Hood", "sphere", (0, 0.96, 0.02), (0.40, 0.36, 0.40), fabric, "Head"),
        create_part("Face Rune", "cone", (0, 0.86, -0.36), (0.20, 0.06, 0.06), rune, "Head", (math.radians(90), 0, 0), 5),
        create_part("Spirit Orb", "sphere", (0, 0.34, -0.48), (0.14, 0.14, 0.14), rune, "Chest"),
        create_part("Staff", "cone", (0.56, 0.26, 0.04), (0.07, 0.07, 0.56), armor, "Hand.R", (0, 0, math.radians(-10)), vertices=6),
        create_part("Staff Crystal", "sphere", (0.66, 0.80, 0.04), (0.16, 0.16, 0.16), rune, "Hand.R"),
    ]
    if female:
        parts.append(create_part("Headdress", "cone", (0, 1.22, 0), (0.28, 0.20, 0.08), armor, "Head", (0, 0, math.radians(14)), vertices=6))
    else:
        parts.append(create_part("Crown", "cone", (0, 1.26, 0), (0.16, 0.26, 0.05), rune, "Head", (0, 0, math.radians(18)), vertices=5))
    return parts


def build_umbra(class_name, gender, armor, fabric, rune, female):
    parts = [
        create_part("Chest Plate", "cone", (0, 0.08, -0.28), (0.44, 0.28, 0.50), armor, "Chest", (math.radians(90), 0, 0)),
        create_part("Void Core", "sphere", (0, 0.26, -0.50), (0.14, 0.14, 0.14), rune, "Chest"),
        create_part("Cape", "cube", (0, 0.04, 0.38), (0.76, 0.82, 0.06), fabric, "Spine", (math.radians(12), 0, 0)),
        create_part("Void Shoulder.L", "cone", (-0.50, 0.48, 0), (0.24, 0.24, 0.06), armor, "Chest", (0, 0, math.radians(22)), vertices=5),
        create_part("Void Shoulder.R", "cone", (0.50, 0.48, 0), (0.24, 0.24, 0.06), armor, "Chest", (0, 0, math.radians(-22)), vertices=5),
        create_part("Void Blade", "cone", (0.52, 0.14, 0.06), (0.12, 0.12, 0.50), armor, "Hand.R", (0, 0, math.radians(-24)), vertices=4),
        create_part("Blade Rune", "cone", (0.30, 0.42, 0.06), (0.06, 0.06, 0.14), rune, "Hand.R", (0, 0, math.radians(-24)), vertices=4),
    ]
    if female:
        parts.append(create_part("Void Tiara", "cone", (0, 1.24, 0), (0.30, 0.22, 0.04), rune, "Head", (0, 0, math.radians(16)), vertices=5))
    else:
        parts.append(create_part("Void Horn.L", "cone", (-0.18, 1.25, 0), (0.12, 0.20, 0.04), rune, "Head", (0, 0, math.radians(-28)), vertices=5))
        parts.append(create_part("Void Horn.R", "cone", (0.18, 1.25, 0), (0.12, 0.20, 0.04), rune, "Head", (0, 0, math.radians(28)), vertices=5))
    return parts


def build_sword(armor, rune, albedo, normal):
    return [
        create_part("Sword Blade", "cone", (0.52, 0.12, -0.18), (0.11, 0.11, 0.46), armor, "Hand.R", (0, 0, math.radians(-26)), vertices=4),
        create_part("Sword Guard", "cone", (0.40, -0.28, -0.18), (0.12, 0.12, 0.06), rune, "Hand.R", (0, 0, math.radians(-26)), vertices=6),
        create_part("Sword Grip", "cone", (0.30, -0.39, -0.18), (0.055, 0.055, 0.14), create_material("Guerrero", "leather", albedo, normal), "Hand.R", (0, 0, math.radians(-26)), vertices=6),
    ]


def export_character(class_name, gender, mesh, armature, weapon_mesh=None, lod_meshes=None):
    os.makedirs(CHARACTER_DIR, exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT")
    mesh.select_set(True)
    if weapon_mesh is not None:
        weapon_mesh.select_set(True)
    for lod_mesh in lod_meshes or []:
        lod_mesh.select_set(True)
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    output = os.path.join(CHARACTER_DIR, f"{class_name}_{gender}.fbx")
    bpy.ops.export_scene.fbx(
        filepath=output,
        use_selection=True,
        object_types={"ARMATURE", "MESH"},
        add_leaf_bones=False,
        bake_anim=True,
        bake_anim_use_all_actions=True,
        bake_anim_force_startend_keying=True,
        path_mode="RELATIVE",
    )
    print(f"EXPORTED {output}")


def main():
    global ALBEDO_IMAGES, NORMAL_IMAGES
    os.makedirs(SOURCE_DIR, exist_ok=True)
    os.makedirs(CHARACTER_DIR, exist_ok=True)
    os.makedirs(TEXTURE_DIR, exist_ok=True)
    build_atlas_pixels()
    ALBEDO_IMAGES, NORMAL_IMAGES = save_atlas_images()

    for class_name in PALETTES:
        for gender in ("Masculino", "Femenino"):
            clear_scene()
            build_character(class_name, gender)

    save_atlas_images(ALBEDO_IMAGES, NORMAL_IMAGES, write_pixels=False)

    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(SOURCE_DIR, "OriginalArt_Source.blend"))
    print("ORIGINAL_ART_EXPORT_COMPLETE")


if __name__ == "__main__":
    main()
