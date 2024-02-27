#!/bin/bash

function nobuild()
{
    mkdir -p ${OUTDIR_ABS}
    
    for file in ${INDIR_ABS}/*.png
	do
		fn=$(basename ${file} .png)
		if [ ! -f ${OUTDIR_ABS}/${fn}.texture ]; then
		    ${TEXPROC} ${INDIR_ABS}/${fn}.png ${OUTDIR_ABS}/${fn}.texture  --generateMipMaps
        fi
	done
}
