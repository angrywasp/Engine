#!/bin/bash

function clean()
{
    rm -vrf ${OUTDIR_ABS}
}

function nobuild()
{
    mkdir -p ${OUTDIR_ABS}
    mkdir -p ${OUTDIR_ABS}/Textures
    mkdir -p ${OUTDIR_ABS}/Meshes
    mkdir -p ${OUTDIR_ABS}/Materials
}