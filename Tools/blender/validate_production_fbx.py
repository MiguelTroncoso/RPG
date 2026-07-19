"""Validate the commercial-art contract for imported FBX assets.

Run with Blender, for example:

    blender --background --python Tools/blender/validate_production_fbx.py -- \
        --all --report docs/production-art-audit.md

This validator is deliberately stricter than Unity's import check. A file can
be skinned, animated and mobile-friendly while still being only a technical
prototype. Commercial approval also requires production provenance, semantic
meshes and a complete PBR material package.
"""

import argparse
import json
import os
import re
import sys
from datetime import datetime

import bpy


PROJECT_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "../.."))
CHARACTER_DIR = os.path.join(PROJECT_ROOT, "Assets/Resources/OriginalArt/Characters")
MOB_DIR = os.path.join(PROJECT_ROOT, "Assets/Resources/OriginalArt/Mobs")

REQUIRED_CLIPS = ("Idle", "Run", "Attack", "Hit", "Death")
REQUIRED_LODS = ("LOD0", "LOD1", "LOD2")
PRIMITIVE_TOKENS = (
    "cone", "cube", "cylinder", "sphere", "icosphere", "plane", "torus",
    "circle", "box", "capsule",
)
REQUIRED_PROVENANCE = (
    "status", "artist_or_vendor", "license", "source_blend", "reviewed_by",
    "reviewed_at",
)


def parse_args():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--all", action="store_true", help="Validate characters and mobs.")
    parser.add_argument("--fbx", action="append", default=[], help="Validate one FBX path.")
    parser.add_argument("--kind", choices=("character", "mob"), default="character")
    parser.add_argument("--report", default="docs/production-art-audit.md")
    parser.add_argument("--json", dest="json_path", default="")
    blender_args = sys.argv
    if "--" in blender_args:
        blender_args = blender_args[blender_args.index("--") + 1:]
    else:
        blender_args = []
    return parser.parse_args(blender_args)


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for action in list(bpy.data.actions):
        bpy.data.actions.remove(action)
    for datablocks in (bpy.data.meshes, bpy.data.armatures, bpy.data.materials, bpy.data.images):
        for datablock in list(datablocks):
            if datablock.users == 0:
                datablocks.remove(datablock)


def import_fbx(path):
    clear_scene()
    bpy.ops.import_scene.fbx(filepath=path, automatic_bone_orientation=False)
    return list(bpy.context.scene.objects)


def image_nodes(materials):
    nodes = []
    for material in materials:
        if material is None or material.node_tree is None:
            continue
        nodes.extend(node for node in material.node_tree.nodes if node.type == "TEX_IMAGE")
    return nodes


def linked_input(materials, names):
    names = {name.lower() for name in names}
    for material in materials:
        if material is None or material.node_tree is None:
            continue
        for node in material.node_tree.nodes:
            if node.type != "BSDF_PRINCIPLED":
                continue
            for name, socket in node.inputs.items():
                if name.lower() in names and socket.is_linked:
                    return True
    return False


def semantic_mesh_name(name):
    lowered = name.lower()
    return not any(re.search(rf"(^|[._ -]){re.escape(token)}([._ -]|$)", lowered)
                   for token in PRIMITIVE_TOKENS)


def read_provenance(path):
    sidecar = os.path.splitext(path)[0] + ".production.json"
    if not os.path.isfile(sidecar):
        return sidecar, {}, ["falta el sidecar de procedencia .production.json"]

    try:
        with open(sidecar, "r", encoding="utf-8") as handle:
            data = json.load(handle)
    except Exception as error:
        return sidecar, {}, [f"sidecar invalido: {error}"]

    failures = [f"falta {key}" for key in REQUIRED_PROVENANCE if not data.get(key)]
    if data.get("status") != "commercial_final":
        failures.append("status no es commercial_final")
    source_blend = data.get("source_blend")
    if source_blend:
        source_path = source_blend if os.path.isabs(source_blend) else os.path.join(PROJECT_ROOT, source_blend)
        if not os.path.isfile(source_path):
            failures.append("source_blend no existe")
    if data.get("reviewed") is not True:
        failures.append("reviewed no esta confirmado")
    return sidecar, data, failures


def validate(path, kind):
    path = os.path.abspath(path)
    result = {
        "asset": os.path.relpath(path, PROJECT_ROOT),
        "kind": kind,
        "passed": False,
        "checks": {},
        "failures": [],
    }
    if not os.path.isfile(path):
        result["failures"].append("el FBX no existe")
        return result

    try:
        objects = import_fbx(path)
    except Exception as error:
        result["failures"].append(f"importacion Blender fallida: {error}")
        return result

    meshes = [obj for obj in objects if obj.type == "MESH"]
    armatures = [obj for obj in objects if obj.type == "ARMATURE"]
    materials = [material for obj in meshes for material in obj.data.materials if material is not None]
    material_names = sorted({material.name for material in materials})
    actions = sorted({action.name for action in bpy.data.actions})
    mesh_names = [obj.name for obj in meshes] + [obj.data.name for obj in meshes]
    primitive_meshes = [name for name in mesh_names if not semantic_mesh_name(name)]
    bones = max((len(armature.data.bones) for armature in armatures), default=0)
    triangle_count = sum(len(mesh.data.loop_triangles) for mesh in meshes)
    for mesh in meshes:
        mesh.data.calc_loop_triangles()
    triangle_count = sum(len(mesh.data.loop_triangles) for mesh in meshes)
    images = image_nodes(materials)
    relative_images = []
    missing_images = []
    for node in images:
        if node.image is None:
            continue
        image_path = bpy.path.abspath(node.image.filepath)
        if image_path:
            relative_images.append(not os.path.isabs(node.image.filepath))
            if not os.path.isfile(image_path):
                missing_images.append(os.path.basename(image_path))

    expected_bones = 20 if kind == "character" else 8
    expected_triangles = 1000 if kind == "character" else 300
    checks = {
        "skinned_armature": bool(armatures) and bones >= expected_bones,
        "armature_modifiers": bool(meshes) and all(
            any(modifier.type == "ARMATURE" for modifier in mesh.modifiers) for mesh in meshes
        ),
        "uvs": bool(meshes) and all(bool(mesh.data.uv_layers) for mesh in meshes),
        "semantic_meshes": bool(meshes) and not primitive_meshes,
        "real_lod_set": all(any(lod.lower() in obj.name.lower() for obj in objects) for lod in REQUIRED_LODS),
        "combat_clips": all(any(clip.lower() in action.lower() for action in actions) for clip in REQUIRED_CLIPS),
        "pbr_albedo_and_normal": bool(images) and linked_input(materials, ("Base Color", "Color"))
        and linked_input(materials, ("Normal",)),
        "pbr_roughness_and_metallic": linked_input(materials, ("Roughness",))
        and linked_input(materials, ("Metallic", "Metalness")),
        "relative_existing_textures": bool(images) and all(relative_images) and not missing_images,
        "reasonable_geometry": triangle_count >= expected_triangles,
    }
    result["checks"] = checks
    result["stats"] = {
        "meshes": len(meshes),
        "armatures": len(armatures),
        "bones": bones,
        "materials": len(material_names),
        "images": len(images),
        "actions": actions,
        "triangles": triangle_count,
        "primitive_mesh_names": sorted(set(primitive_meshes)),
    }
    sidecar, provenance, provenance_failures = read_provenance(path)
    result["provenance"] = {"sidecar": os.path.relpath(sidecar, PROJECT_ROOT), **provenance}
    for name, passed in checks.items():
        if not passed:
            result["failures"].append(name)
    result["failures"].extend(provenance_failures)
    result["passed"] = not result["failures"]
    return result


def asset_groups(args):
    if args.fbx:
        return [(path, args.kind) for path in args.fbx]
    if args.all:
        character_paths = [
            os.path.join(CHARACTER_DIR, name)
            for name in sorted(os.listdir(CHARACTER_DIR))
            if name.lower().endswith(".fbx")
        ]
        mob_paths = [
            os.path.join(MOB_DIR, name)
            for name in sorted(os.listdir(MOB_DIR))
            if name.lower().endswith(".fbx")
        ]
        return [(path, "character") for path in character_paths] + [(path, "mob") for path in mob_paths]
    raise SystemExit("Usa --all o al menos un --fbx")


def write_report(results, report_path):
    passed = [result for result in results if result["passed"]]
    character_results = [result for result in results if result["kind"] == "character"]
    mob_results = [result for result in results if result["kind"] == "mob"]

    def summary(items):
        return f"{sum(item['passed'] for item in items)}/{len(items)} aprobados"

    lines = [
        "# Auditoria de arte 3D comercial",
        "",
        f"> Generado por `validate_production_fbx.py` el {datetime.now():%Y-%m-%d %H:%M}.",
        "",
        "## Resultado",
        "",
        f"- Personajes: **{summary(character_results)}**.",
        f"- Mobs y jefes: **{summary(mob_results)}**.",
        f"- Total: **{len(passed)}/{len(results)}** aprobados.",
        "",
        "Un FBX skinned con animaciones no se considera arte comercial final por si solo. La aprobacion exige procedencia, meshes semanticos, LOD real, UV, materiales PBR completos, texturas autocontenidas y revision visual.",
        "",
        "## Detalle",
        "",
        "| Estado | Tipo | Asset | Fallos |",
        "| --- | --- | --- | --- |",
    ]
    for result in results:
        state = "PASS" if result["passed"] else "PENDIENTE"
        failures = ", ".join(result["failures"]) if result["failures"] else ""
        lines.append(f"| {state} | {result['kind']} | `{result['asset']}` | {failures} |")
    lines.extend([
        "",
        "## Regla de salida",
        "",
        "El runtime puede seguir usando KayKit/Quaternius mientras esta auditoria no tenga un lote comercial aprobado. No se debe activar `OriginalArt` tecnico solo para hacer que el contador parezca completo.",
        "",
        "## Contrato",
        "",
        "Consultar `docs/final-art-production.md` antes de entregar un FBX nuevo.",
    ])
    destination = os.path.join(PROJECT_ROOT, report_path) if not os.path.isabs(report_path) else report_path
    os.makedirs(os.path.dirname(destination), exist_ok=True)
    with open(destination, "w", encoding="utf-8") as handle:
        handle.write("\n".join(lines) + "\n")


def main():
    args = parse_args()
    results = [validate(path, kind) for path, kind in asset_groups(args)]
    write_report(results, args.report)
    if args.json_path:
        destination = os.path.join(PROJECT_ROOT, args.json_path) if not os.path.isabs(args.json_path) else args.json_path
        os.makedirs(os.path.dirname(destination), exist_ok=True)
        with open(destination, "w", encoding="utf-8") as handle:
            json.dump(results, handle, indent=2, ensure_ascii=False)
    print(f"PRODUCTION_ART characters={sum(r['passed'] for r in results if r['kind'] == 'character')}/{sum(r['kind'] == 'character' for r in results)} mobs={sum(r['passed'] for r in results if r['kind'] == 'mob')}/{sum(r['kind'] == 'mob' for r in results)} total={sum(r['passed'] for r in results)}/{len(results)}")
    return 0 if all(result["passed"] for result in results) else 2


if __name__ == "__main__":
    main()
