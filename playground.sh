for i in /home/hirbod/projects/bingx_bot/src/bot/src/*; do
    if [[ ! -d "$i" ]]; then
        continue
    fi

    dir=$i
    file=${i##*/}

    echo $dir
    echo $file

    if [[ $file == "Configuration" || $file == "PnLAnalysis" || $file == "Properties" ]]; then
        continue
    fi

    if [[ -d "$i" ]]; then
        fdir=/home/hirbod/projects/bingx_bot/lib/$file/src
        if [[ $file == "Data" ]]; then
            fdir=/home/hirbod/projects/bingx_bot/lib/Repositories/src
        fi
        dir=$dir/*
        cp -r $dir $fdir
    fi
done
