#!/bin/bash


function clean() {
    rm -vrf ${OUTDIR_ABS}
}

function build() {
    mkdir -p ${OUTDIR_ABS}/Meshes
    mkdir -p ${OUTDIR_ABS}/CollisionMeshes
    mkdir -p ${OUTDIR_ABS}/Textures
    mkdir -p ${OUTDIR_ABS}/Materials

    for file in ${INDIR_ABS}/Meshes/*.glb; do
        fn=$(basename ${file} .glb)
        if [ ! -f ${OUTDIR_ABS}/Meshes/${fn}.mesh ]; then
            ${MESHPROC} --root ${ENGINE_ROOT_DIR} --input "${INDIR_ABS}/Meshes/${fn}.glb" \
            --meshOutput "${OUTDIR_REL}/Meshes" \
            --textureOutput "${OUTDIR_REL}/Textures" \
            --materialOutput "${OUTDIR_REL}/Materials" --mipmaps
        fi
    done

    for file in ${INDIR_ABS}/CollisionMeshes/*.glb; do
        fn=$(basename ${file} .glb)
        if [ ! -f ${OUTDIR_ABS}/CollisionMeshes/${fn}.mesh ]; then
            ${MESHPROC} --root ${ENGINE_ROOT_DIR} --input "${INDIR_ABS}/CollisionMeshes/${fn}.glb" \
            --meshOutput "${OUTDIR_REL}/CollisionMeshes" --collision
        fi
    done
}
