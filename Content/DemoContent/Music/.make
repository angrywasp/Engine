#!/bin/bash

function clean()
{
    rm -vrf ${OUTDIR_ABS}
}

function convert()
{
    for file in ${INDIR_ABS}/*.ogg
	do
        ffmpeg -acodec libvorbis -i "${file}" -acodec pcm_s16le "${file%ogg}wav"
	done
}

function build()
{
	mkdir -p ${OUTDIR_ABS}
    
    for file in ${INDIR_ABS}/*.wav
	do
        fn=$(basename ${file} .wav)
		if [ ! -f ${OUTDIR_ABS}/${fn}.sound ]; then
		    ${SOUNDPROC} -i ${INDIR_ABS}/${fn}.wav -o ${OUTDIR_ABS}/${fn}.sound -c 1
        fi
	done
}
