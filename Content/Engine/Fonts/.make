#!/bin/bash

function clean()
{
	rm -vrf ${OUTDIR_ABS}
}

function build()
{
    mkdir -p ${OUTDIR_ABS}
    if [ ! -f ${OUTDIR_ABS}/Default.fontpkg ]; then
	    ${FONTPROC} --input "${INDIR_ABS}/ProFontWindows.ttf" --output "${OUTDIR_ABS}/Default.fontpkg" \
        --numbers --lower --upper --symbols --space 2 \
        --sizes "12,14,16,18,24,30,36,48,72,96,120"
    fi
}

function fonts()
{
    build
}
