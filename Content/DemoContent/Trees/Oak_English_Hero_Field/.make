#!/bin/bash

function clean() {
    rm -vrf ${OUTDIR_ABS}
}

function trees() {
    name=Oak_English_Hero_Field

    mkdir -p ${OUTDIR_ABS}
    mkdir -p ${OUTDIR_ABS}/Meshes
    mkdir -p ${OUTDIR_ABS}/Textures
    mkdir -p ${OUTDIR_ABS}/Materials

    defaultEmissive="Engine/Textures/Default_emissive.texture"

    ${MATPACK} --width 2048 --height 2048 --name ${name}_Bark --output "${OUTDIR_ABS}/Textures" --exclude "emissive" \
        --albedo "${INDIR_ABS}/Textures/Oak_bark_3_albedo.png" \
        --normal "${INDIR_ABS}/Textures/Oak_bark_3_normal.png" \
        --pbr "${INDIR_ABS}/Textures/Oak_bark_3_pbr.png" \
        --mipmaps --exclude "emissive"

    ${MATPACK} --width 4096 --height 4096 --name ${name}_Leaf --output "${OUTDIR_ABS}/Textures" --exclude "emissive" \
        --albedo "${INDIR_ABS}/Textures/Oak_English_Hero_Field_albedo.png" \
        --normal "${INDIR_ABS}/Textures/Oak_English_Hero_Field_normal.png" \
        --pbr "${INDIR_ABS}/Textures/Oak_English_Hero_Field_pbr.png" \
        --mipmaps --exclude "emissive"

    ${MATGEN} --albedo "DemoContent/Trees/${name}/Textures/${name}_Leaf_albedo.texture" \
        --normal "DemoContent/Trees/${name}/Textures/${name}_Leaf_normal.texture" \
        --pbr "DemoContent/Trees/${name}/Textures/${name}_Leaf_specular.texture" \
        --emissive ${defaultEmissive} \
        --output "${OUTDIR_ABS}/Materials/${name}_Leaf.material"

    ${MATGEN} --albedo "DemoContent/Trees/${name}/Textures/${name}_Bark_albedo.texture" \
        --normal "DemoContent/Trees/${name}/Textures/${name}_Bark_normal.texture" \
        --pbr "DemoContent/Trees/${name}/Textures/${name}_Bark_specular.texture" \
        --emissive ${defaultEmissive} \
        --output "${OUTDIR_ABS}/Materials/${name}_Bark.material"

    ${MESHPROC} --input "${INDIR_ABS}/Meshes/${name}.glb" --output "${OUTDIR_ABS}/Meshes/${name}.mesh"
}
