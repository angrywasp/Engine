#!/bin/bash

function build()
{
	mkdir -p ${OUTDIR_ABS}

	for file in ${INDIR_ABS}/*.glb
	do
		fn=$(basename ${file} .glb)
        if [ ! -f ${OUTDIR_ABS}/${fn}.mesh ]; then
		    ${MESHPROC} --root ${ENGINE_ROOT_DIR} --input "${INDIR_ABS}/${fn}.glb" --meshOutput "${OUTDIR_REL}"
        fi
	done

    if [ ! -f ${OUTDIR_ABS}/LightMeshRaw.mesh ]; then
	    ${MESHMERGE} ${OUTDIR_ABS}/LightSphere.mesh ${OUTDIR_ABS}/LightCone.mesh ${OUTDIR_ABS}/LightMeshRaw.mesh
    fi
}

function meshes() {
    build
}
