#!/bin/bash

function nobuild()
{
    mkdir -p ${OUTDIR_ABS}

    if [ ! -f "${OUTDIR_ABS}/Bark_albedo.texture" ]; then
        ${MATTEXPROC} --width 512 --height 1024 --name Bark --output "${OUTDIR_ABS}" \
            --albedo "${INDIR_ABS}/Bark_d.png" \
            --normal "${INDIR_ABS}/Bark_n.png" \
            --specular "${INDIR_ABS}/Bark_s.png" \
            --exclude "pbr;emissive" --mipmaps
    fi

    if [ ! -f "${OUTDIR_ABS}/Leaf_albedo.texture" ]; then
        ${MATTEXPROC} --width 1024 --height 1024 --name Leaf --output "${OUTDIR_ABS}" \
            --albedo "${INDIR_ABS}/leaf_d.png" \
            --specular "${INDIR_ABS}/Leaf_s.png" \
            --exclude "pbr;emissive" --mipmaps
    fi
}
