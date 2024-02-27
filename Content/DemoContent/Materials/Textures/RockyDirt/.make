#!/bin/bash

function clean()
{
	rm -vrf ${OUTDIR_ABS}
}

function build()
{
    name=RockyDirt
    texPrefix=rocky_dirt1

    mkdir -p ${OUTDIR_ABS}
    mkdir -p ${OUTDIR_ABS}

    if [ ! -f "${OUTDIR_ABS}/${name}_pbr.texture" ]; then
        ${MATPACK} --name ${name} --output "${OUTDIR_ABS}" \
            --albedo "${INDIR_ABS}/${texPrefix}-albedo.png" \
            --normal "${INDIR_ABS}/${texPrefix}-normal-ogl.png" \
            --roughness "${INDIR_ABS}/${texPrefix}_Roughness.png" \
            --displacement "${INDIR_ABS}/${texPrefix}_Height.png" \
            --mipmaps --exclude "emissive"
    fi
}

function materials()
{
    build
}
