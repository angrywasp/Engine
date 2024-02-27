#!/bin/bash

function clean()
{
	rm -vrf ${OUTDIR_ABS}
}

function build()
{
    name=DirtWithRocks
    texPrefix=dirtwithrocks

    mkdir -p ${OUTDIR_ABS}
    mkdir -p ${OUTDIR_ABS}

    if [ ! -f "${OUTDIR_ABS}/${name}_pbr.texture" ]; then
        ${MATPACK} --name ${name} --output "${OUTDIR_ABS}" \
            --albedo "${INDIR_ABS}/${texPrefix}_Base_Color.png" \
            --normal "${INDIR_ABS}/${texPrefix}_Normal-ogl.png" \
            --ao "${INDIR_ABS}/${texPrefix}_Ambient_Occlusion.png" \
            --roughness "${INDIR_ABS}/${texPrefix}_Roughness.png" \
            --displacement "${INDIR_ABS}/${texPrefix}_Height.png" \
            --mipmaps --exclude "emissive"
    fi
}

function materials()
{
    build
}
