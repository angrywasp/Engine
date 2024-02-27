#!/bin/bash

function nobuild()
{
    mkdir -p ${OUTDIR_ABS}
    
	TEXDIR="DemoContent/Entities/ViperScout/Textures"
	DE="Engine/Textures/Default_emissive.texture"

    TEXTURES=("Chassis" "Doors" "Guns1" "Guns2" "Interior" "Suspension" "Tools" "Wheel")

    for tx in "${TEXTURES[@]}"
    do
        if [ ! -f ${OUTDIR_ABS}/$tx.material ]; then
		    ${MATGEN} -d "${TEXDIR}/${tx}_d.texture" -n "${TEXDIR}/${tx}_n.texture" -s "${TEXDIR}/${tx}_s.texture" -e $DE -o "${OUTDIR_ABS}/$tx.material"
        fi
    done
}
