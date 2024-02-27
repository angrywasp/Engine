#!/bin/bash

function nobuild()
{
	mkdir -p ${OUTDIR_ABS}

	for file in ${INDIR_ABS}/*.glb
	do
		fn=$(basename ${file} .glb)
        if [ ! -f ${OUTDIR_ABS}/${fn}.mesh ]; then
		    ${MESHPROC} --input "${INDIR_ABS}/${fn}.glb" --output "${OUTDIR_ABS}/${fn}.mesh"
        fi
	done
}

function meshes() {
    build
}
