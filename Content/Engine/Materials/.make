#!/bin/bash

function clean() {
    rm -vrf ${OUTDIR_ABS}
}

function build() {
    width=1
    height=1

    mkdir -p ${OUTDIR_ABS}
    mkdir -p ${OUTDIR_ABS}/Colors

    defaultAlbedo="Engine/Textures/Default_albedo.texture"
    defaultNormal="Engine/Textures/Default_normal.texture"
    defaultPbr="Engine/Textures/DefaultRoughness255_pbr.texture"
    defaultEmissive="Engine/Textures/Default_emissive.texture"

    if [ ! -f "${OUTDIR_ABS}/Default.material" ]; then
        ${MATGEN} --albedo ${defaultAlbedo} --normal ${defaultNormal} --pbr ${defaultPbr} --emissive ${defaultEmissive} --output "${OUTDIR_ABS}/Default.material"
    fi

    mats=(
        Black
        Blue
        Cyan
        DeepSkyBlue
        Gray
        Green
        GreenYellow
        Gold
        Magenta
        Orange
        Pink
        Purple
        Red
        White
        Yellow
    )

    for mat in "${mats[@]}"; do
        if [ ! -f "${OUTDIR_ABS}/Colors/${mat}.material" ]; then
            ${MATGEN} --albedo "Engine/Textures/Colors/${mat}_albedo.texture" --normal ${defaultNormal} --pbr ${defaultPbr} --emissive ${defaultEmissive} --output "${OUTDIR_ABS}/Colors/${mat}.material"
        fi
    done
}

function materials() {
    build
}
