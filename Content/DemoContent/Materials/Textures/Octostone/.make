#!/bin/bash

function clean()
{
	rm -vrf ${OUTDIR_ABS}
}

function build()
{
    name=Octostone
    texPrefix=octostone

    mkdir -p ${OUTDIR_ABS}
    mkdir -p ${OUTDIR_ABS}

    if [ ! -f "${OUTDIR_ABS}/${name}_pbr.texture" ]; then
        ${MATPACK} --name ${name} --output "${OUTDIR_ABS}" \
            --albedo "${INDIR_ABS}/${texPrefix}Albedo.png" \
            --normal "${INDIR_ABS}/${texPrefix}Normalc.png" \
            --ao "${INDIR_ABS}/${texPrefix}Ambient_Occlusionc.png" \
            --roughness "${INDIR_ABS}/${texPrefix}Roughness2.png" \
            --displacement "${INDIR_ABS}/${texPrefix}Heightc.png" \
            --mipmaps --exclude "emissive"
    fi
}

function materials()
{
    build
}
