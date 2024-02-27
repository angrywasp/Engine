#!/bin/bash

function clean()
{
	rm -vrf ${OUTDIR_ABS}
}

function build()
{
    mkdir -p ${OUTDIR_ABS}
    
    if [ ! -f ${OUTDIR_ABS}/Chiller.fontpkg ]; then
	    ${FONTPROC} --input "${INDIR_ABS}/Chiller.ttf" --output "${OUTDIR_ABS}/Chiller.fontpkg" \
        --numbers --lower --upper --symbols --space 2 \
        --sizes "12,14,16,18,24,30,36,48,72,96,120"
    fi
}

function fonts()
{
    build
}
