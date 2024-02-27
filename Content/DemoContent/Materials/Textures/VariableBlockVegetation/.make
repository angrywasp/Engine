#!/bin/bash

function clean()
{
	rm -vrf ${OUTDIR_ABS}
}

function build()
{
    name=VariableBlocksVegetation
    texPrefix=variable-blocks-vegetation

    mkdir -p ${OUTDIR_ABS}
    mkdir -p ${OUTDIR_ABS}

    if [ ! -f "${OUTDIR_ABS}/${name}_pbr.texture" ]; then
        ${MATPACK} --name ${name} --output "${OUTDIR_ABS}" \
            --albedo "${INDIR_ABS}/${texPrefix}_albedo.png" \
            --normal "${INDIR_ABS}/${texPrefix}_normal-ogl.png" \
            --ao "${INDIR_ABS}/${texPrefix}_ao.png" \
            --roughness "${INDIR_ABS}/${texPrefix}_roughness.png" \
            --displacement "${INDIR_ABS}/${texPrefix}_height.png" \
            --mipmaps --exclude "emissive"
    fi
}

function materials()
{
    build
}
