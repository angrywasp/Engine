#!/bin/bash

function nobuild()
{
    mkdir -p ${OUTDIR_ABS}

    TEXDIR="DemoContent/Maps/Corridor/Textures"
    DE="Engine/Textures/Default_emissive.texture"

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
        if [ ! -f ${OUTDIR_ABS}/${name}.material ]; then
		    ${MATGEN} --albedo "${TEXDIR}/${name}_albedo.texture" --normal "${TEXDIR}/${name}_normal.texture" --pbr "${TEXDIR}/${name}_pbr.texture" --emissive ${DE} --output "${OUTDIR_ABS}/${name}.material"
        fi
	done
}
