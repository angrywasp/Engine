#!/bin/bash -e

function clean()
{
	rm -r ${OUTDIR_ABS}
}

function build()
{
	files=()
	for file in ${INDIR_ABS}/*.cs
	do
		fn=$(basename ${file} .cs)
		files+=(${file})
	done

	${MACROPROC} ${files[@]}
}

function macros() {
    build
}
