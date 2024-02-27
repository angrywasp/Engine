#!/bin/bash

function clean()
{
	rm -vrf ${OUTDIR_ABS}
}

function build()
{
    mkdir -p ${OUTDIR_ABS}
    if [ ! -f ${OUTDIR_ABS}/SpawnPoint.mesh ]; then
        ${MESHPROC} --root ${ENGINE_ROOT_DIR} --input "${INDIR_ABS}/SpawnPoint.glb" --meshOutput "${OUTDIR_REL}"
    fi
}
