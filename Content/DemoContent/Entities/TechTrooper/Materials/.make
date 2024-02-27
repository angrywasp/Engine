#!/bin/bash

function nobuild()
{
    mkdir -p ${OUTDIR_ABS}

	TEXDIR="DemoContent/Entities/TechTrooper/Textures"

    if [ ! -f "${OUTDIR_ABS}/TechTrooper_LowerBody.material" ]; then
		${MATGEN} -d "${TEXDIR}/TechTrooper_LowerBody_d.texture" -n "${TEXDIR}/TechTrooper_LowerBody_n.texture" -s "${TEXDIR}/TechTrooper_LowerBody_s.texture" -e "${TEXDIR}/TechTrooper_LowerBody_e.texture" -o "${OUTDIR_ABS}/TechTrooper_LowerBody.material"
    fi

    if [ ! -f "${OUTDIR_ABS}/TechTrooper_Torso.material" ]; then
		${MATGEN} -d "${TEXDIR}/TechTrooper_Torso_d.texture" -n "${TEXDIR}/TechTrooper_Torso_n.texture" -s "${TEXDIR}/TechTrooper_Torso_s.texture" -e "${TEXDIR}/TechTrooper_Torso_e.texture" -o "${OUTDIR_ABS}/TechTrooper_Torso.material"
    fi
}
