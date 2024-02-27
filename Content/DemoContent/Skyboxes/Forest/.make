#!/bin/bash

maps=(Brudslojan Langholmen2 Langholmen3 MountainPath Plants)

function build() {
    mkdir -p ${OUTDIR_ABS}
    mkdir -p ${OUTDIR_ABS}/Textures

    for i in "${maps[@]}"; do
        if [ ! -f ${OUTDIR_ABS}/Textures/$i.texcube ]; then
            ${TEXCUBEPROC} \
            --posX ${INDIR_ABS}/$i/posx.jpg \
            --negX ${INDIR_ABS}/$i/negx.jpg \
            --posY ${INDIR_ABS}/$i/posy.jpg \
            --negY ${INDIR_ABS}/$i/negy.jpg \
            --posZ ${INDIR_ABS}/$i/posz.jpg \
            --negZ ${INDIR_ABS}/$i/negz.jpg \
            --output ${OUTDIR_ABS}/Textures/$i.texcube \
            --mipmaps --irradiance --reflectance
        fi
    done
}

function skyboxes() {
    build
}
