#!/bin/bash

function clean()
{
    rm -r ${OUTDIR_ABS}
}

function build()
{
    mkdir -p ${OUTDIR_ABS}
    cp -r ${INDIR_ABS}/* ${OUTDIR_ABS}
}
