#!/bin/bash

function clean()
{
	rm -vrf ${OUTDIR_ABS}
}

function build()
{
    name=WetStonesWithSand
    texPrefix=wet-stones-with-sand1

    mkdir -p ${OUTDIR_ABS}
    mkdir -p ${OUTDIR_ABS}

    if [ ! -f "${OUTDIR_ABS}/${name}_pbr.texture" ]; then
        ${MATPACK} --name ${name} --output "${OUTDIR_ABS}" \
            --albedo "${INDIR_ABS}/${texPrefix}-albedo.png" \
            --normal "${INDIR_ABS}/${texPrefix}-normal-ogl.png" \
            --ao "${INDIR_ABS}/${texPrefix}-ao.png" \
            --roughness "${INDIR_ABS}/${texPrefix}-roughness.png" \
            --displacement "${INDIR_ABS}/${texPrefix}-height.png" \
            --mipmaps --exclude "emissive"
    fi
}

function materials()
{
    build
}
