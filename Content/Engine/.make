#!/bin/bash

function clean() {
    rm -vrf ${OUTDIR_ABS}
}

function build() {
    ${TEXGEN} --output "${OUTDIR_ABS}"
}

function materials() {
    build
}
