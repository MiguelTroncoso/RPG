import math
import os

import bpy


PROJECT_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "../.."))
MOB_DIR = os.path.join(PROJECT_ROOT, "Assets/Resources/OriginalArt/Mobs")
TEXTURE_DIR = os.path.join(PROJECT_ROOT, "Assets/Resources/OriginalArt/Textures")
SOURCE_DIR = os.path.join(PROJECT_ROOT, "Assets/Art/Original")
ATLAS_SIZE = 2048
TILE_SIZE = 256

ZONES = [
    ("valley", (0.25, 0.12, 0.07, 1.0), (0.95, 0.52, 0.16, 1.0), "beast"),
    ("forest", (0.08, 0.30, 0.14, 1.0), (0.22, 0.82, 0.30, 1.0), "beast"),
    ("ash", (0.28, 0.08, 0.04, 1.0), (1.0, 0.26, 0.06, 1.0), "shadow"),
    ("crystal", (0.12, 0.26, 0.42, 1.0), (0.28, 0.82, 1.0, 1.0), "guardian"),
    ("frost", (0.20, 0.40, 0.54, 1.0), (0.72, 0.94, 1.0, 1.0), "beast"),
    ("sunken", (0.04, 0.30, 0.28, 1.0), (0.12, 0.92, 0.82, 1.0), "spirit"),
    ("obsidian", (0.12, 0.045, 0.035, 1.0), (1.0, 0.22, 0.06, 1.0), "guardian"),
    ("astral", (0.14, 0.08, 0.30, 1.0), (0.62, 0.42, 1.0, 1.0), "spirit"),
    ("eclipse", (0.10, 0.025, 0.18, 1.0), (0.86, 0.22, 1.0, 1.0), "shadow"),
    ("throne", (0.20, 0.04, 0.28, 1.0), (1.0, 0.42, 0.82, 1.0), "guardian"),
]
TIERS = (("normal", 1.0), ("elite", 1.08), ("boss", 1.18))
MATERIAL_CACHE = {}
ALBEDO_IMAGE = None
NORMAL_IMAGE = None


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.armatures, bpy.data.cameras, bpy.data.lights):
        for datablock in list(datablocks):
            if datablock.users == 0:
                datablocks.remove(datablock)
    for action in list(bpy.data.actions):
        bpy.data.actions.remove(action)


def clamp(value):
    return max(0.0, min(1.0, value))


def write_pixel(pixels, x, y, color):
    index = (y * ATLAS_SIZE + x) * 4
    pixels[index:index + 4] = color


def build_atlas():
    global ALBEDO_IMAGE, NORMAL_IMAGE
    albedo_pixels = [0.0] * (ATLAS_SIZE * ATLAS_SIZE * 4)
    normal_pixels = [0.5, 0.5, 1.0, 1.0] * (ATLAS_SIZE * ATLAS_SIZE)
    for slot, (zone, base, accent, _) in enumerate(ZONES):
        tile_x = (slot % 8) * TILE_SIZE
        tile_y = (slot // 8) * TILE_SIZE
        for y in range(TILE_SIZE):
            for x in range(TILE_SIZE):
                u = (x + 0.5) / TILE_SIZE
                v = (y + 0.5) / TILE_SIZE
                broad = math.sin((u * 7.0 + v * 3.0 + slot) * math.pi) * 0.045
                fine = math.sin((u * 37.0 - v * 29.0 + slot * 0.7) * math.pi) * 0.025
                edge = math.sin(u * math.pi * 12.0) * math.sin(v * math.pi * 10.0) * 0.035
                mix = clamp(0.12 + 0.18 * ((math.sin((u + v + slot) * math.pi * 3.0) + 1.0) * 0.5))
                painted = [clamp(channel * (0.9 + broad + fine + edge) + accent[i] * mix) for i, channel in enumerate(base[:3])]
                height = 0.5 + math.sin((u * 15.0 + slot) * math.pi) * 0.06 + math.cos((v * 19.0 - slot) * math.pi) * 0.04
                write_pixel(albedo_pixels, tile_x + x, tile_y + y, painted + [1.0])
                left = math.sin(((x - 1) / TILE_SIZE * 15.0 + slot) * math.pi) * 0.06
                right = math.sin(((x + 1) / TILE_SIZE * 15.0 + slot) * math.pi) * 0.06
                down = math.cos(((y - 1) / TILE_SIZE * 19.0 - slot) * math.pi) * 0.04
                up = math.cos(((y + 1) / TILE_SIZE * 19.0 - slot) * math.pi) * 0.04
                nx = clamp(0.5 - (right - left) * 1.7)
                ny = clamp(0.5 - (up - down) * 1.7)
                write_pixel(normal_pixels, tile_x + x, tile_y + y, [nx, ny, 1.0, 1.0])

    os.makedirs(TEXTURE_DIR, exist_ok=True)
    ALBEDO_IMAGE = bpy.data.images.get("OriginalArt_Mobs_AlbedoAtlas_2K") or bpy.data.images.new(
        "OriginalArt_Mobs_AlbedoAtlas_2K", width=ATLAS_SIZE, height=ATLAS_SIZE, alpha=True
    )
    NORMAL_IMAGE = bpy.data.images.get("OriginalArt_Mobs_NormalAtlas_2K") or bpy.data.images.new(
        "OriginalArt_Mobs_NormalAtlas_2K", width=ATLAS_SIZE, height=ATLAS_SIZE, alpha=True
    )
    ALBEDO_IMAGE.pixels.foreach_set(albedo_pixels)
    NORMAL_IMAGE.pixels.foreach_set(normal_pixels)
    NORMAL_IMAGE.colorspace_settings.name = "Non-Color"
    ALBEDO_IMAGE.filepath_raw = os.path.join(TEXTURE_DIR, "OriginalArt_Mobs_AlbedoAtlas_2K.png")
    NORMAL_IMAGE.filepath_raw = os.path.join(TEXTURE_DIR, "OriginalArt_Mobs_NormalAtlas_2K.png")
    ALBEDO_IMAGE.file_format = "PNG"
    NORMAL_IMAGE.file_format = "PNG"
    ALBEDO_IMAGE.save()
    NORMAL_IMAGE.save()


def make_atlas_material(name, slot, metallic=0.12, roughness=0.54):
    cache_key = (name, slot, metallic, roughness)
    if cache_key in MATERIAL_CACHE:
        return MATERIAL_CACHE[cache_key]
    material = bpy.data.materials.new(name)
    material.use_nodes = True
    nodes = material.node_tree.nodes
    links = material.node_tree.links
    nodes.clear()
    output = nodes.new("ShaderNodeOutputMaterial")
    bsdf = nodes.new("ShaderNodeBsdfPrincipled")
    uv = nodes.new("ShaderNodeTexCoord")
    mapping = nodes.new("ShaderNodeMapping")
    albedo = nodes.new("ShaderNodeTexImage")
    normal_texture = nodes.new("ShaderNodeTexImage")
    normal_map = nodes.new("ShaderNodeNormalMap")
    albedo.image = ALBEDO_IMAGE
    normal_texture.image = NORMAL_IMAGE
    normal_texture.image.colorspace_settings.name = "Non-Color"
    tile_x = (slot % 8) / 8.0
    tile_y = (slot // 8) / 8.0
    mapping.inputs["Scale"].default_value = (0.125, 0.125, 1.0)
    mapping.inputs["Location"].default_value = (tile_x, tile_y, 0.0)
    links.new(uv.outputs["UV"], mapping.inputs["Vector"])
    links.new(mapping.outputs["Vector"], albedo.inputs["Vector"])
    links.new(mapping.outputs["Vector"], normal_texture.inputs["Vector"])
    links.new(albedo.outputs["Color"], bsdf.inputs["Base Color"])
    links.new(normal_texture.outputs["Color"], normal_map.inputs["Color"])
    links.new(normal_map.outputs["Normal"], bsdf.inputs["Normal"])
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    links.new(bsdf.outputs["BSDF"], output.inputs["Surface"])
    MATERIAL_CACHE[cache_key] = material
    return material


def make_solid_material(name, color, metallic=0.2, roughness=0.42):
    material = bpy.data.materials.new(name)
    material.diffuse_color = color
    material.use_nodes = True
    bsdf = material.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = color
        bsdf.inputs["Metallic"].default_value = metallic
        bsdf.inputs["Roughness"].default_value = roughness
        bsdf.inputs["Emission Color"].default_value = tuple(color[:3]) + (1.0,)
        bsdf.inputs["Emission Strength"].default_value = 0.22
    return material


def make_armature():
    data = bpy.data.armatures.new("OriginalMobRig")
    armature = bpy.data.objects.new("OriginalMobRig", data)
    bpy.context.collection.objects.link(armature)
    bpy.context.view_layer.objects.active = armature
    armature.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bones = {}

    def bone(name, head, tail, parent=None):
        item = data.edit_bones.new(name)
        item.head = head
        item.tail = tail
        if parent:
            item.parent = bones[parent]
        bones[name] = item

    bone("Root", (0, 0, 0), (0, 0.2, 0))
    bone("Hips", (0, -0.30, 0), (0, 0.04, 0), "Root")
    bone("Spine", (0, 0.04, 0), (0, 0.34, 0), "Hips")
    bone("Chest", (0, 0.34, 0), (0, 0.62, 0), "Spine")
    bone("Neck", (0, 0.62, 0), (0, 0.80, 0), "Chest")
    bone("Head", (0, 0.80, 0), (0, 1.18, 0), "Neck")
    for side, sign in (("L", -1), ("R", 1)):
        bone(f"UpperArm.{side}", (sign * 0.42, 0.38, 0), (sign * 0.58, 0.08, 0), "Chest")
        bone(f"LowerArm.{side}", (sign * 0.58, 0.08, 0), (sign * 0.58, -0.23, 0), f"UpperArm.{side}")
        bone(f"Hand.{side}", (sign * 0.58, -0.23, 0), (sign * 0.58, -0.40, 0), f"LowerArm.{side}")
        bone(f"Thigh.{side}", (sign * 0.20, -0.30, 0), (sign * 0.20, -0.68, 0), "Hips")
        bone(f"Foot.{side}", (sign * 0.20, -0.68, 0), (sign * 0.20, -0.98, -0.12), f"Thigh.{side}")
    bpy.ops.object.mode_set(mode="POSE")
    for pose_bone in armature.pose.bones:
        pose_bone.rotation_mode = "XYZ"
    bpy.ops.object.mode_set(mode="OBJECT")
    armature.display_type = "WIRE"
    return armature


def assign_uvs(obj):
    uv_layer = obj.data.uv_layers.new(name="UVMap")
    for loop in obj.data.loops:
        coordinate = obj.data.vertices[loop.vertex_index].co
        uv_layer.data[loop.index].uv = (0.5 + coordinate.x * 0.45, 0.5 + coordinate.z * 0.45)


def create_part(name, kind, location, scale, material, bone, rotation=(0, 0, 0)):
    if kind == "sphere":
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=1.0, location=location)
    elif kind == "cone":
        bpy.ops.mesh.primitive_cone_add(vertices=8, radius1=1.0, radius2=0.62, depth=1.0, location=location)
    elif kind == "cylinder":
        bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=1.0, depth=1.0, location=location)
    else:
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    obj.rotation_euler = rotation
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    if kind == "cube":
        bevel = obj.modifiers.new("Authored bevel", "BEVEL")
        bevel.width = min(min(obj.dimensions) * 0.18, 0.06)
        bevel.segments = 2
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_apply(modifier=bevel.name)
    for polygon in obj.data.polygons:
        polygon.use_smooth = kind in ("sphere", "cone", "cylinder")
    obj.data.materials.append(material)
    assign_uvs(obj)
    group = obj.vertex_groups.new(name=bone)
    group.add(list(range(len(obj.data.vertices))), 1.0, "REPLACE")
    return obj


def join_parts(parts, armature, name):
    bpy.ops.object.select_all(action="DESELECT")
    for part in parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    mesh = bpy.context.object
    mesh.name = name
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.remove_doubles(threshold=0.0001)
    bpy.ops.mesh.normals_make_consistent(inside=False)
    bpy.ops.object.mode_set(mode="OBJECT")
    modifier = mesh.modifiers.new("Original Mob Rig Skin", "ARMATURE")
    modifier.object = armature
    mesh.parent = armature
    mesh["original_art"] = True
    mesh["art_pass"] = "all_zones_authored_mob_v1"
    mesh["retopology_version"] = "android_target_cage"
    mesh["uv_layout"] = "shared_mob_atlas_2K"
    mesh["normal_bake_source"] = "procedural_high_poly_detail"
    return mesh


def create_lod(source, armature, name, ratio):
    lod = source.copy()
    lod.data = source.data.copy()
    lod.name = name
    bpy.context.collection.objects.link(lod)
    lod.parent = armature
    decimate = lod.modifiers.new("Android geometric LOD", "DECIMATE")
    decimate.ratio = ratio
    bpy.context.view_layer.objects.active = lod
    bpy.ops.object.select_all(action="DESELECT")
    lod.select_set(True)
    bpy.ops.object.modifier_apply(modifier=decimate.name)
    lod["lod_ratio"] = ratio
    return lod


def add_actions(armature, prefix):
    modes = {
        "Idle": ((1, 20, 40, 60), {"Spine": (0.025, 0, 0), "Head": (0, 0, 0.03)}),
        "Run": ((1, 6, 11, 16), {"UpperArm.L": (-0.45, 0, 0), "UpperArm.R": (0.45, 0, 0), "Thigh.L": (0.55, 0, 0), "Thigh.R": (-0.55, 0, 0)}),
        "Attack": ((1, 6, 12, 20), {"Spine": (0.18, 0, -0.18), "Chest": (0.25, 0, -0.25), "UpperArm.R": (1.0, 0, 0.22), "LowerArm.R": (0.55, 0, 0)}),
        "Hit": ((1, 4, 9, 14), {"Spine": (0, 0, 0.2), "Chest": (-0.12, 0, 0.16), "Head": (0.1, 0, -0.08)}),
        "Death": ((1, 8, 16, 26), {"Hips": (0.2, 0, 0.12), "Spine": (0.38, 0, -0.24), "Chest": (0.44, 0, -0.28), "Head": (-0.3, 0, 0.18)}),
    }
    for mode, (frames, targets) in modes.items():
        action = bpy.data.actions.new(f"{prefix}_{mode}")
        action.use_fake_user = True
        action["animation_pass"] = "authored_mob_combat_v1"
        action["suitable_for_android"] = True
        armature.animation_data_create()
        slot = action.slots.new("OBJECT", armature.name)
        layer = action.layers.new("Base")
        strip = layer.strips.new(type="KEYFRAME")
        channelbag = strip.channelbag(slot, ensure=True)
        for bone_name, target in targets.items():
            for axis in range(3):
                curve = channelbag.fcurves.new(f'pose.bones["{bone_name}"].rotation_euler', index=axis, group_name=bone_name)
                for index, frame in enumerate(frames):
                    phase = (0.0, 1.0, -0.6, 0.0)[index]
                    if mode == "Attack":
                        phase = (0.0, 0.4, 1.0, 0.0)[index]
                    elif mode == "Death":
                        phase = (0.0, 0.35, 0.8, 1.0)[index]
                    curve.keyframe_points.insert(frame, target[axis] * phase)
        if mode == "Death":
            location = channelbag.fcurves.new('pose.bones["Hips"].location', index=2, group_name="Hips")
            for frame, value in zip(frames, (0.0, -0.06, -0.18, -0.32)):
                location.keyframe_points.insert(frame, value)
        armature.animation_data.action = action
        armature.animation_data.action_slot = slot


def zone_identity(parts, zone, tier, body, accent, dark, armature):
    if zone == "valley":
        parts.append(create_part("Relic collar", "cylinder", (0, 0.55, 0), (0.40, 0.07, 0.40), accent, "Chest"))
        parts.append(create_part("Relic rune", "sphere", (0, 0.48, -0.44), (0.16, 0.16, 0.08), accent, "Chest"))
    elif zone == "forest":
        parts.append(create_part("Root mantle", "cone", (0, 0.22, 0.30), (0.72, 0.25, 0.12), dark, "Spine"))
        parts.append(create_part("Antler L", "cone", (-0.28, 1.12, 0), (0.10, 0.38, 0.10), accent, "Head", (0, 0, -0.35)))
        parts.append(create_part("Antler R", "cone", (0.28, 1.12, 0), (0.10, 0.38, 0.10), accent, "Head", (0, 0, 0.35)))
    elif zone == "ash":
        parts.append(create_part("Ember core", "sphere", (0, 0.32, -0.45), (0.22, 0.22, 0.10), accent, "Chest"))
        parts.append(create_part("Ash plume", "cone", (0.0, 1.28, 0), (0.18, 0.35, 0.18), accent, "Head", (0.2, 0, 0)))
    elif zone == "crystal":
        parts.append(create_part("Crystal core", "cone", (0, 0.40, -0.44), (0.18, 0.28, 0.12), accent, "Chest"))
        parts.append(create_part("Crystal shoulder L", "cone", (-0.48, 0.52, 0), (0.14, 0.36, 0.14), accent, "Chest", (0, 0, -0.25)))
        parts.append(create_part("Crystal shoulder R", "cone", (0.48, 0.52, 0), (0.14, 0.36, 0.14), accent, "Chest", (0, 0, 0.25)))
    elif zone == "frost":
        parts.append(create_part("Frost crest", "cone", (0, 1.28, 0), (0.16, 0.42, 0.16), accent, "Head"))
        parts.append(create_part("Frost plate", "cube", (0, 0.34, -0.40), (0.56, 0.22, 0.08), accent, "Chest"))
    elif zone == "sunken":
        parts.append(create_part("Abyss pearl", "sphere", (0, 0.36, -0.44), (0.20, 0.20, 0.12), accent, "Chest"))
        parts.append(create_part("Fin L", "cone", (-0.44, 0.26, 0.18), (0.10, 0.42, 0.10), accent, "Spine", (0, 0, -0.4)))
        parts.append(create_part("Fin R", "cone", (0.44, 0.26, 0.18), (0.10, 0.42, 0.10), accent, "Spine", (0, 0, 0.4)))
    elif zone == "obsidian":
        parts.append(create_part("Forge ring", "cylinder", (0, 0.64, 0), (0.62, 0.04, 0.62), accent, "Chest", (math.pi / 2, 0, 0)))
        parts.append(create_part("Magma core", "sphere", (0, 0.36, -0.45), (0.22, 0.22, 0.10), accent, "Chest"))
        parts.append(create_part("Forge spike", "cone", (0.52, 0.45, 0), (0.12, 0.42, 0.12), accent, "Hand.R", (0, 0, -0.4)))
    elif zone == "astral":
        parts.append(create_part("Astral orb", "sphere", (0, 1.36, -0.08), (0.20, 0.20, 0.20), accent, "Head"))
        parts.append(create_part("Astral ring", "cylinder", (0, 0.72, 0), (0.76, 0.035, 0.76), accent, "Spine", (math.pi / 2, 0, 0)))
    elif zone == "eclipse":
        parts.append(create_part("Eclipse disc", "cylinder", (0, 1.22, 0), (0.44, 0.06, 0.44), dark, "Head", (math.pi / 2, 0, 0)))
        parts.append(create_part("Void halo", "cylinder", (0, 1.22, 0), (0.68, 0.03, 0.68), accent, "Head", (math.pi / 2, 0, 0)))
    else:
        parts.append(create_part("Throne crown", "cylinder", (0, 1.28, 0), (0.50, 0.08, 0.50), accent, "Head"))
        parts.append(create_part("Throne mantle", "cube", (0, 0.34, 0.30), (0.96, 0.38, 0.10), dark, "Spine", (0.2, 0, 0)))

    if tier == "elite":
        parts.append(create_part("Elite crest", "cone", (0, 1.20, 0), (0.14, 0.28, 0.14), accent, "Head"))
        parts.append(create_part("Elite shoulder L", "sphere", (-0.52, 0.40, 0), (0.20, 0.16, 0.24), accent, "Chest"))
        parts.append(create_part("Elite shoulder R", "sphere", (0.52, 0.40, 0), (0.20, 0.16, 0.24), accent, "Chest"))
    elif tier == "boss":
        parts.append(create_part("Boss halo", "cylinder", (0, 1.38, 0), (0.72, 0.035, 0.72), accent, "Head", (math.pi / 2, 0, 0)))
        parts.append(create_part("Boss horn L", "cone", (-0.32, 1.18, 0), (0.14, 0.42, 0.14), accent, "Head", (0, 0, -0.38)))
        parts.append(create_part("Boss horn R", "cone", (0.32, 1.18, 0), (0.14, 0.42, 0.14), accent, "Head", (0, 0, 0.38)))


def build_model(zone_info, tier_name, tier_scale, slot):
    zone, base, accent, style = zone_info
    armature = make_armature()
    body = make_atlas_material(f"Mob_{zone}_{tier_name}_Atlas", slot, metallic=0.22 if style == "guardian" else 0.08)
    rune = make_solid_material(f"Mob_{zone}_{tier_name}_Accent", accent, metallic=0.34, roughness=0.28)
    dark = make_solid_material(f"Mob_{zone}_{tier_name}_Shadow", tuple(channel * 0.42 for channel in base[:3]) + (1.0,), metallic=0.12, roughness=0.64)
    parts = []
    if style == "beast":
        parts += [
            create_part("Beast torso", "sphere", (0, 0.04, 0), (0.58, 0.48, 0.78), body, "Spine"),
            create_part("Beast head", "sphere", (0, 0.22, 0.68), (0.46, 0.42, 0.46), body, "Head"),
            create_part("Beast muzzle", "cube", (0, 0.10, 1.02), (0.28, 0.18, 0.22), dark, "Head"),
            create_part("Beast leg L", "cylinder", (-0.30, -0.46, 0.34), (0.16, 0.42, 0.16), dark, "Thigh.L"),
            create_part("Beast leg R", "cylinder", (0.30, -0.46, 0.34), (0.16, 0.42, 0.16), dark, "Thigh.R"),
            create_part("Beast rear L", "cylinder", (-0.30, -0.46, -0.34), (0.16, 0.42, 0.16), dark, "Thigh.L"),
            create_part("Beast rear R", "cylinder", (0.30, -0.46, -0.34), (0.16, 0.42, 0.16), dark, "Thigh.R"),
            create_part("Beast ear L", "cone", (-0.22, 0.68, 0.66), (0.11, 0.25, 0.11), rune, "Head", (0, 0, -0.28)),
            create_part("Beast ear R", "cone", (0.22, 0.68, 0.66), (0.11, 0.25, 0.11), rune, "Head", (0, 0, 0.28)),
            create_part("Beast tail", "cylinder", (0, 0.18, -0.82), (0.11, 0.42, 0.11), rune, "Spine", (-0.7, 0, 0)),
        ]
    elif style == "guardian":
        parts += [
            create_part("Guardian core", "cube", (0, 0.02, 0), (0.68, 0.86, 0.54), body, "Spine"),
            create_part("Guardian head", "cube", (0, 0.78, 0), (0.48, 0.40, 0.46), dark, "Head"),
            create_part("Guardian visor", "cube", (0, 0.78, -0.25), (0.34, 0.08, 0.05), rune, "Head"),
            create_part("Guardian shoulder L", "cube", (-0.50, 0.38, 0), (0.26, 0.28, 0.56), rune, "Chest", (0, 0, -0.16)),
            create_part("Guardian shoulder R", "cube", (0.50, 0.38, 0), (0.26, 0.28, 0.56), rune, "Chest", (0, 0, 0.16)),
            create_part("Guardian arm L", "cylinder", (-0.57, -0.02, 0), (0.14, 0.34, 0.14), dark, "LowerArm.L"),
            create_part("Guardian arm R", "cylinder", (0.57, -0.02, 0), (0.14, 0.34, 0.14), dark, "LowerArm.R"),
            create_part("Guardian leg L", "cube", (-0.22, -0.70, 0), (0.24, 0.40, 0.28), dark, "Thigh.L"),
            create_part("Guardian leg R", "cube", (0.22, -0.70, 0), (0.24, 0.40, 0.28), dark, "Thigh.R"),
            create_part("Guardian core gem", "sphere", (0, 0.20, -0.33), (0.15, 0.15, 0.10), rune, "Chest"),
        ]
    elif style == "spirit":
        parts += [
            create_part("Spirit body", "sphere", (0, -0.02, 0), (0.56, 0.84, 0.44), body, "Spine"),
            create_part("Spirit hood", "sphere", (0, 0.86, 0), (0.44, 0.42, 0.44), dark, "Head"),
            create_part("Spirit face", "cube", (0, 0.84, -0.25), (0.28, 0.08, 0.05), rune, "Head"),
            create_part("Spirit sash", "cube", (0, 0.06, -0.40), (0.64, 0.12, 0.07), rune, "Chest"),
            create_part("Spirit orb", "sphere", (0, 0.40, -0.46), (0.15, 0.15, 0.15), rune, "Chest"),
            create_part("Spirit skirt", "cone", (0, -0.56, 0), (0.68, 0.30, 0.50), dark, "Hips"),
        ]
    else:
        parts += [
            create_part("Shadow body", "sphere", (0, 0, 0), (0.54, 0.84, 0.42), body, "Spine"),
            create_part("Shadow hood", "sphere", (0, 0.90, 0), (0.44, 0.44, 0.44), dark, "Head"),
            create_part("Shadow cloak", "cube", (0, 0.08, 0.26), (0.76, 0.86, 0.10), dark, "Spine", (0.18, 0, 0)),
            create_part("Shadow eye line", "cube", (0, 0.91, -0.34), (0.30, 0.07, 0.05), rune, "Head"),
            create_part("Shadow blade", "cone", (0.48, 0.10, -0.08), (0.08, 0.54, 0.08), rune, "Hand.R", (0, 0, -0.30)),
            create_part("Shadow leg L", "cylinder", (-0.20, -0.68, 0), (0.16, 0.42, 0.16), dark, "Thigh.L"),
            create_part("Shadow leg R", "cylinder", (0.20, -0.68, 0), (0.16, 0.42, 0.16), dark, "Thigh.R"),
        ]

    zone_identity(parts, zone, tier_name, body, rune, dark, armature)
    mesh = join_parts(parts, armature, f"OriginalMob_{zone}_{tier_name}_LOD0")
    lods = [create_lod(mesh, armature, f"OriginalMob_{zone}_{tier_name}_LOD1", 0.55), create_lod(mesh, armature, f"OriginalMob_{zone}_{tier_name}_LOD2", 0.28)]
    prefix = f"OriginalMob_{zone}_{tier_name}"
    add_actions(armature, prefix)
    armature.scale = (tier_scale, tier_scale, tier_scale)
    armature["zone"] = zone
    armature["tier"] = tier_name
    armature["art_pass"] = "all_zones_authored_mob_v1"
    armature["animation_set"] = "idle_run_attack_hit_death"
    os.makedirs(MOB_DIR, exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT")
    for item in [mesh] + lods:
        item.select_set(True)
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    output = os.path.join(MOB_DIR, f"{zone}_{tier_name}.fbx")
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
    os.makedirs(SOURCE_DIR, exist_ok=True)
    os.makedirs(MOB_DIR, exist_ok=True)
    build_atlas()
    slot = 0
    for zone_info in ZONES:
        for tier_name, tier_scale in TIERS:
            clear_scene()
            build_model(zone_info, tier_name, tier_scale, slot)
            slot += 1
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(SOURCE_DIR, "OriginalMob_Source.blend"))
    print("ORIGINAL_MOB_EXPORT_COMPLETE 30 MODELS")


if __name__ == "__main__":
    main()
