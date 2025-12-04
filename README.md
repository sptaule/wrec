# wrec

Un enregistreur d'écran pour Windows.

### Fonctionnalités

- [x] Enregistre l'écran entier ou bien une zone définie
- [x] Résolution paramétrable
- [x] Encodeur paramétrable (framerate, profil, bitrate, qualité)
- [x] Peut enregistrer l'audio système
- [x] Peut enregistrer l'audio microphone
- [x] Modification des volumes audio (système et micro)
- [x] Peut capturer le curseur et les clics de souris
- [x] Paramétrage des clics de souris (couleurs, rayon, durée)
- [x] Peut ajouter un timer avant l'enregistrement
- [x] Raccourcis clavier pour démarrer/arrêter l'enregistrement (<kbd>Ctrl+F9</kbd> et <kbd>Ctrl+F10</kbd>)
- [x] Icône SystemTray lors de l'enregistrement
- [x] Compression disponible à la fin de l'enregistrement (ffmpeg)
- [x] Configuration sauvegardée à chaque changement

### Librairies utilisées

- .NET ~ Windows Forms
- ScreenRecorderLib
- MaterialSkin2

### Compression

A la fin de l'enregistrement, la taille du fichier sera affichée.  
Vous pourrez alors choisir de compresser la vidéo si FFmpeg est disponible dans votre environnement.  

La détection et la sélection sera automatique pour l'encodeur en fonction de ce qui est disponible dans votre environnement (av1_nvenc, hevc_nvenc, h264_nvenc, libx265).  
Vous pourrez choisir le taux de compression.  
L'avancement de la compression est indiqué avec une jauge de progression.  
A la fin d'une compression, il est indiqué le gain obtenu sur le fichier original, et il est également possible de recommencer une compression en choisissant un autre preset.

### Resources utilisées

| Statut       | CPU            | RAM           |
| :---:        |     :---:      |         :---: |
| idle         | 0.1%           | ~17 MB        |
| recording    | 1%             | ~100 MB       |

### Conclusion

Il s'agit de mon premier programme C# donc des bugs peuvent être présents.  
Avant tout créé pour mon usage personnel, je souhaitais un programme simple permettant d'enregistrer mon écran entier avec audio système et micro.  
Je remercie grandement la librairie [ScreenRecorderLib](https://github.com/sskodje/ScreenRecorderLib) sans quoi ce programme n'aurait pas vu le jour.
