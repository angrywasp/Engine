#!/bin/bash

function nobuild()
{
    mkdir -p ${OUTDIR_ABS}

    names=(
        CeilingCenter
        CeilingCorner_IN
        Ceiling_Corner_OUT
        CeilingSide
        FloorCenter
        FloorCorner_IN
        FloorCorner_OUT
        FloorSide
        WallAA
        WallBA
        WallCorner
    )
    
    for name in ${names[@]}; do
        if [ ! -f "${OUTDIR_ABS}/${name}_pbr.texture" ]; then
            ${MATPACK} --name ${name} --output "${OUTDIR_ABS}" \
                --albedo "${INDIR_ABS}/${name}_Albedo.png" \
                --normal "${INDIR_ABS}/${name}_Normal.png" \
                --roughness "${INDIR_ABS}/${name}_Roughness.png" \
                --mipmaps --exclude "emissive"
        fi
	done
}
