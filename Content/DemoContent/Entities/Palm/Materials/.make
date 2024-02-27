#!/bin/bash

function nobuild()
{
    mkdir -p ${OUTDIR_ABS}

    TEXDIR="DemoContent/Entities/Palm/Textures"
	TEXDIR_D="Engine/Textures"

    if [ ! -f "${OUTDIR_ABS}/Bark.material" ]; then
        ${MATGEN} --albedo "${TEXDIR}/Bark_albedo.texture" --normal "${TEXDIR}/Bark_normal.texture" -s "${TEXDIR}/Bark_specular.texture" -e "${TEXDIR_D}/Default_emissive.texture" -o "${OUTDIR_ABS}/Bark.material"
    fi

    if [ ! -f "${OUTDIR_ABS}/Leaf.material" ]; then
        ${MATGEN} --albedo "${TEXDIR}/Leaf_albedo.texture" --normal "${TEXDIR_D}/Default_normal.texture" -s "${TEXDIR}/Leaf_specular.texture" -e "${TEXDIR_D}/Default_emissive.texture" -o "${OUTDIR_ABS}/Leaf.material" -x
    fi
}
