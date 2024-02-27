#!/bin/bash

function clean() {
    rm -vrf ${OUTDIR_ABS}
}

function build()
{
    name=GrassLowRes
    width=512
    height=512

    mkdir -p ${OUTDIR_ABS}
    mkdir -p ${OUTDIR_ABS}

    if [ ! -f "${OUTDIR_ABS}/${name}_pbr.texture" ]; then
        ${MATPACK} --name ${name} --output "${OUTDIR_ABS}" \
            --albedo "${INDIR_ABS}/${name}_albedo.png" \
            --normal "${INDIR_ABS}/${name}_normal.png" \
            --ao "${INDIR_ABS}/${name}_ao.png" \
            --roughness "${INDIR_ABS}/${name}_roughness.png" \
            --mipmaps --invertNormals --exclude "emissive"
    fi
}

textures()
{
    build
}
