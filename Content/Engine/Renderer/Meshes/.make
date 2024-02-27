#!/bin/bash

function clean() {
    rm -vrf ${OUTDIR_ABS}
}

function buildInternal()
{
    name=$1
    if [ ! -f "${OUTDIR_ABS}/${name}.mesh" ]; then
        ${MESHPROC} --root ${ENGINE_ROOT_DIR} --input "${INDIR_ABS}/${name}.glb" --meshOutput "${OUTDIR_REL}"
        ${MESHPROC} --root ${ENGINE_ROOT_DIR} --input "${INDIR_ABS}/${name}.glb" --meshOutput "${OUTDIR_REL}" --flip
    fi
}

function buildInternalRecalculate()
{
    name=$1
    if [ ! -f "${OUTDIR_ABS}/${name}.mesh" ]; then
        ${MESHPROC} --root ${ENGINE_ROOT_DIR} --input "${INDIR_ABS}/${name}.glb" --meshOutput "${OUTDIR_REL}" --recalculate
        ${MESHPROC} --root ${ENGINE_ROOT_DIR} --input "${INDIR_ABS}/${name}.glb" --meshOutput "${OUTDIR_REL}" --flip --recalculate
    fi
}

function build()
{
	mkdir -p ${OUTDIR_ABS}

    buildInternal Cube
    buildInternal Sphere
    buildInternal Cylinder
}

function meshes() {
    build
}
