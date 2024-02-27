#!/bin/bash

function clean()
{
    rm -r ${OUTDIR_ABS}
}

function build()
{
    mkdir -p ${OUTDIR_ABS}
    cp -r ${INDIR_ABS}/* ${OUTDIR_ABS}

    #cp ${OUTDIR_ABS}/Mesh/RenderToGBuffer.frag.glsl ${OUTDIR_ABS}/Mesh/RenderToGBuffer.skinned.frag.glsl
    #cp ${OUTDIR_ABS}/Mesh/Reconstruct.frag.glsl ${OUTDIR_ABS}/Mesh/Reconstruct.skinned.frag.glsl
    #cp ${OUTDIR_ABS}/Mesh/ShadowMap.frag.glsl ${OUTDIR_ABS}/Mesh/ShadowMap.skinned.frag.glsl
}

function shaders()
{
    build
}
