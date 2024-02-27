#!/bin/bash

function clean()
{
	rm -vrf ${OUTDIR_ABS}
}

function build()
{
    name=WornShinyMetal
    texPrefix=worn-shiny-metal

    mkdir -p ${OUTDIR_ABS}
    mkdir -p ${OUTDIR_ABS}

    if [ ! -f "${OUTDIR_ABS}/${name}_pbr.texture" ]; then
        ${MATPACK} --name ${name} --output "${OUTDIR_ABS}" \
            --albedo "${INDIR_ABS}/${texPrefix}-albedo.png" \
            --normal "${INDIR_ABS}/${texPrefix}-Normal-ogl.png" \
            --metalness "${INDIR_ABS}/${texPrefix}-Metallic.png" \
            --roughness "${INDIR_ABS}/${texPrefix}-Roughness.png" \
            --displacement "${INDIR_ABS}/${texPrefix}-Height.png" \
            --mipmaps --exclude "emissive"
    fi
}

materials()
{
    build
}
