#!/bin/bash

function clean()
{
    rm -r ${OUTDIR_ABS}
}

function build()
{
    mkdir -p ${OUTDIR_ABS}
    cp -r ${INDIR_ABS}/Atari ${OUTDIR_ABS}
	cp -r ${INDIR_ABS}/NES ${OUTDIR_ABS}
	cp -r ${INDIR_ABS}/C64 ${OUTDIR_ABS}
}
