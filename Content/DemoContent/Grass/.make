#!/bin/bash

function clean()
{
	rm -vrf ${OUTDIR_ABS}
}

function build()
{
    mkdir -p ${OUTDIR_ABS}

    if [ ! -f "${OUTDIR_ABS}/Grass.material" ]; then
        ${MATGEN} --albedo "DemoContent/Grass/Textures/GrassLowRes_albedo.texture" --normal "DemoContent/Grass/Textures/GrassLowRes_normal.texture" \
        --pbr "DemoContent/Grass/Textures/GrassLowRes_pbr.texture" --emissive "Engine/Textures/Default_emissive.texture" --output "${OUTDIR_ABS}/Grass.material" --doubleSided
    fi
}

function materials()
{
    build
}
