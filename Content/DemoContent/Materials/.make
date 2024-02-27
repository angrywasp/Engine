#!/bin/bash

function clean() {
    rm -vrf ${OUTDIR_ABS}
}

function build() {
    mkdir -p ${OUTDIR_ABS}

    texDir="DemoContent/Materials/Textures"

    mats=(
        AngledBlocksVegetation
        DirtWithRocks
        DryDirt1
        DryDirt2
        GrassyMeadow
        HexagonPavers
        MixedMoss
        PatchyMeadow
        RiverRocks
        RockSlabWall
        RockyDirt
        RockyWornGround
        RustedIron
        SquareBlocksVegetation
        WetStonesWithSand
        WornShinyMetal
        ScuffedCopper
        ScuffedGold
        ScuffedIron
    )

    emissiveMats=(
        ColumnedLavaRock
        LavaAndRock
    )

    for mat in "${mats[@]}"; do
        if [ ! -f "${OUTDIR_ABS}/${mat}.material" ]; then
            ${MATGEN} --albedo "${texDir}/${mat}/${mat}_albedo.texture" --normal "${texDir}/${mat}/${mat}_normal.texture" --pbr "${texDir}/${mat}/${mat}_pbr.texture" --emissive "Engine/Textures/Default_emissive.texture" --output "${OUTDIR_ABS}/${mat}.material"
        fi
    done

    for mat in "${emissiveMats[@]}"; do
        if [ ! -f "${OUTDIR_ABS}/${mat}.material" ]; then
            ${MATGEN} --albedo "${texDir}/${mat}/${mat}_albedo.texture" --normal "${texDir}/${mat}/${mat}_normal.texture" --pbr "${texDir}/${mat}/${mat}_pbr.texture" --emissive "${texDir}/${mat}/${mat}_emissive.texture" --output "${OUTDIR_ABS}/${mat}.material"
        fi
    done
}

function materials() {
    build
}
