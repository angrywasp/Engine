#!/bin/bash

function clean()
{
    rm -vrf ${OUTDIR_ABS}
}

function build()
{
    mkdir -p ${OUTDIR_ABS}
}
