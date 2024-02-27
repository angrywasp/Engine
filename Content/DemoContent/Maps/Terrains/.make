#!/bin/bash

function clean()
{
    rm -vrf ${OUTDIR_ABS}
}

function build()
{
    mkdir -p ${OUTDIR_ABS}

    if [ ! -f ${OUTDIR_ABS}/Heightmap1_513.terrain ]; then
        ${HMAPPROC} --input ${INDIR_ABS}/Heightmap1_513.png --output ${OUTDIR_ABS}/Heightmap1_513.terrain \
        --scale 75.0 --smooth 10
    fi

    if [ ! -f ${OUTDIR_ABS}/Heightmap1_1025.terrain ]; then
        ${HMAPPROC} --input ${INDIR_ABS}/Heightmap1_1025.png --output ${OUTDIR_ABS}/Heightmap1_1025.terrain \
        --scale 75.0 --smooth 10 --erode 0
    fi

    if [ ! -f ${OUTDIR_ABS}/Heightmap2_1025.terrain ]; then
        ${HMAPPROC} --input ${INDIR_ABS}/Heightmap2_1025.png --output ${OUTDIR_ABS}/Heightmap2_1025.terrain \
        --scale 100.0
    fi

    if [ ! -f ${OUTDIR_ABS}/Heightmap1_513_Blend.texture ]; then
        ${TEXPROC} --input ${INDIR_ABS}/Heightmap1_513_Blend.png --output ${OUTDIR_ABS}/Heightmap1_513_Blend.texture
    fi

    if [ ! -f ${OUTDIR_ABS}/Heightmap1_1025_Blend.texture ]; then
        ${TEXPROC} --input ${INDIR_ABS}/Heightmap1_1025_Blend.png --output ${OUTDIR_ABS}/Heightmap1_1025_Blend.texture
    fi

    if [ ! -f ${OUTDIR_ABS}/Heightmap2_1025_Blend.texture ]; then
        ${TEXPROC} --input ${INDIR_ABS}/Heightmap2_1025_Blend.png --output ${OUTDIR_ABS}/Heightmap2_1025_Blend.texture
    fi
}
