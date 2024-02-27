#!/bin/bash

function clean()
{
	rm -vrf ${OUTDIR_ABS}
}

function build()
{
    name=RiverRocks
    texPrefix=river_rock1

    mkdir -p ${OUTDIR_ABS}
    mkdir -p ${OUTDIR_ABS}

    if [ ! -f "${OUTDIR_ABS}/${name}_pbr.texture" ]; then
        ${MATPACK} --name ${name} --output "${OUTDIR_ABS}" \
            --albedo "${INDIR_ABS}/${texPrefix}_albedo.png" \
            --normal "${INDIR_ABS}/${texPrefix}_Normal-ogl.png" \
            --ao "${INDIR_ABS}/${texPrefix}_ao.png" \
            --roughness "${INDIR_ABS}/${texPrefix}_Roughness.png" \
            --displacement "${INDIR_ABS}/${texPrefix}_Height.png" \
            --mipmaps --exclude "emissive"
    fi
}

function materials()
{
    build
}
