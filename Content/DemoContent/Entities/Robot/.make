#!/bin/bash

function clean() {
    rm -vrf ${OUTDIR_ABS}
}

function build() {
    mkdir -p ${OUTDIR_ABS}
    mkdir -p ${OUTDIR_ABS}/Textures
    mkdir -p ${OUTDIR_ABS}/Meshes
    mkdir -p ${OUTDIR_ABS}/Materials

    if [ ! -f "${OUTDIR_ABS}/Meshes/Robot.mesh" ]; then
        ${MESHPROC} --root ${ENGINE_ROOT_DIR} --input "${INDIR_ABS}/Robot.glb" --meshOutput "${OUTDIR_REL}/Meshes" --textureOutput "${OUTDIR_REL}/Textures" --materialOutput "${OUTDIR_REL}/Materials"
    fi
}
