#!/bin/bash

function build() {
    mkdir -p ${OUTDIR_ABS}
    mkdir -p ${OUTDIR_ABS}/Textures

    for i in {0..9}; do
        if [ ! -f ${OUTDIR_ABS}/Textures/$i.texcube ]; then
            ${TEXCUBEPROC} \
            --posX ${INDIR_ABS}/$i/right.png \
            --negX ${INDIR_ABS}/$i/left.png \
            --posY ${INDIR_ABS}/$i/top.png \
            --negY ${INDIR_ABS}/$i/bottom.png \
            --posZ ${INDIR_ABS}/$i/front.png \
            --negZ ${INDIR_ABS}/$i/back.png \
            --output ${OUTDIR_ABS}/Textures/$i.texcube \
            --mipmaps --irradiance --reflectance
        fi
    done
}

function skyboxes() {
    build
}
