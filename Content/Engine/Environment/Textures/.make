
#!/bin/bash

function clean()
{
    rm -vrf ${OUTDIR_ABS}/*.texture
}

function build()
{
    mkdir -p ${OUTDIR_ABS}
    
    for file in ${INDIR_ABS}/*.png
	do
		fn=$(basename ${file} .png)
        if [ ! -f ${OUTDIR_ABS}/${fn}.texture ]; then
		    ${TEXPROC} --input ${INDIR_ABS}/${fn}.png --output ${OUTDIR_ABS}/${fn}.texture --mipmaps
        fi
	done
}
