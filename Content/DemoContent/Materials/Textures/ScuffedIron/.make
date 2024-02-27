#!/bin/bash

function clean()
{
	rm -vrf ${OUTDIR_ABS}
}

function build()
{
    name=ScuffedIron
    texPrefix=Iron-Scuffed

    mkdir -p ${OUTDIR_ABS}
    mkdir -p ${OUTDIR_ABS}

    if [ ! -f "${OUTDIR_ABS}/${name}_pbr.texture" ]; then
        ${MATPACK} --name ${name} --output "${OUTDIR_ABS}" \
            --albedo "${INDIR_ABS}/${texPrefix}_basecolor.png" \
            --metalness "${INDIR_ABS}/${texPrefix}_metallic.png" \
            --roughness "${INDIR_ABS}/${texPrefix}_roughness.png" \
            --mipmaps --exclude "emissive"
    fi
}

function materials()
{
    build
}
