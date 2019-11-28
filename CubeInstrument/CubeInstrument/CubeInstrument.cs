using Sifteo;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CubeInstrument
{
    public class CubeInstrument : BaseApp
    {
        // Diese beiden Arrays werden f�r das allgemeine Spiel ben�tigt:
        
        public Sound[] Note = new Sound[18]; 
        // Array f�r die Noten-Sounds

        public String[] NoteBilder = new String[18];
        // Array f�r die Bilder, die f�r die Noten auf dem Cube dargestellt werden sollen
        


        // Diese Arrays und Variablen werden f�r das Tonfolge-Abfrage-Spiel ben�tigt:
        
        public Sound[] randomTonfolge; 
        // Array, in dem die zuf�llige Tonfolge abgespeichert wird
        
        public Sound[] randomTonfolgeAbfrage; 
        // Array, in dem die mit den Cubes erstellte Tonfolge gespeichert wird, um sie mit der zuf�lligen Tonfolge zu vergleichen
        
        public Sound[] SpielSounds = new Sound[2]; 
        // Array mit Sounds, die bei richtiger/falscher Anordnung der Noten/Cubes abgespielt werden sollen
        
        public bool SpielL�uft; 
        // Boolean, der ausgibt, ob das Tonfolge-Abfrage-Spiel gespielt wird oder nicht
        
        public int Schwierigkeitsstufe; 
        // Integer, der bei jeder richtig zusammengesetzten Tonfolge hochz�hlt, um so das Tonfolge-Abfrage-Spiel schwieriger zu machen

        public int randomTonfolgePosition; 
        // Integer, der dabei hilft, die Arrays f�r das Tonfolge-Abfrage-Spiel mit Sounds zu bef�llen


        // FrameRate bestimmt Geschwindigkeit, wie schnell die Methode "Tick" wiederholt werden soll
        override public int FrameRate
        {
            get { return 2; }
        }

        //  called during intitialization, before the game has started to run
        override public void Setup()
        {
            // Manuelles Bef�llen der Arrays
            Note[0] = Sounds.CreateSound("c3");
            Note[1] = Sounds.CreateSound("h2");
            Note[2] = Sounds.CreateSound("a2");
            Note[3] = Sounds.CreateSound("g2");
            Note[4] = Sounds.CreateSound("f2");
            Note[5] = Sounds.CreateSound("e2");
            Note[6] = Sounds.CreateSound("d2");
            Note[7] = Sounds.CreateSound("c2");
            Note[8] = Sounds.CreateSound("h1");
            Note[9] = Sounds.CreateSound("a1");
            Note[10] = Sounds.CreateSound("g1");
            Note[11] = Sounds.CreateSound("f1");
            Note[12] = Sounds.CreateSound("e1");
            Note[13] = Sounds.CreateSound("d1");
            Note[14] = Sounds.CreateSound("c1");
            Note[15] = Sounds.CreateSound("h");
            Note[16] = Sounds.CreateSound("a");
            Note[17] = Sounds.CreateSound("g");

            NoteBilder[0] = "c3";
            NoteBilder[1] = "h2";
            NoteBilder[2] = "a2";
            NoteBilder[3] = "g2";
            NoteBilder[4] = "f2";
            NoteBilder[5] = "e2";
            NoteBilder[6] = "d2";
            NoteBilder[7] = "c2";
            NoteBilder[8] = "h1";
            NoteBilder[9] = "a1";
            NoteBilder[10] = "g1";
            NoteBilder[11] = "f1";
            NoteBilder[12] = "e1";
            NoteBilder[13] = "d1";
            NoteBilder[14] = "c1";
            NoteBilder[15] = "h";
            NoteBilder[16] = "a";
            NoteBilder[17] = "g";

            SpielSounds[0] = Sounds.CreateSound("FamilienDuell falsche Antwort");
            SpielSounds[1] = Sounds.CreateSound("Fanfare Win");



            Log.Debug("" + CubeSet.Count);

            // Festlegen der L�nge der Arrays (= Anzahl der Cubes, die angeschlossen sind):
            randomTonfolge = new Sound[CubeSet.Count];
            randomTonfolgeAbfrage = new Sound[CubeSet.Count];


            // Jeder Cube bekommt die Daten und Funktionen, die �ber die Klasse "W�rfel" definiert wurden
            foreach (Cube cube in CubeSet)
            {
                W�rfel w�rfel = new W�rfel(this, cube);
                w�rfel.mNote = Note;
            }

            // Zus�tzlich bekommt das Spiel noch folgende Funktionen:

            this.PauseEvent += OnPause;
            this.UnpauseEvent += OnUnpause;
            // Verhalten, wenn das Spiel pausiert/fortgesetzt wird

            CubeSet.NewCubeEvent += OnNewCube;
            CubeSet.LostCubeEvent += OnLostCube;
            // Verhalten, wenn ein Cube au�erhalb der Reichweite liegt oder neu dazukommt.

            Log.Debug("Setup()");
        }

        // Das Spiel wird pausiert:
        private void OnPause()
        {
            // Jedem Cube werden die allgemeinen Funktionen weggenommen und ein "Standby"-Bild wird auf den Cubes angezeigt
            foreach (Cube CuInCube in CubeSet)
            {
                W�rfel w�rfel = (W�rfel)CuInCube.userData;
                CuInCube.ButtonEvent -= w�rfel.OnButton;
                CuInCube.TiltEvent -= w�rfel.OnTilt;
                CuInCube.ShakeStartedEvent -= w�rfel.OnShakeStarted;
                CuInCube.ShakeStartedEvent += w�rfel.ShakePause;
                CuInCube.Image("YoshiForCubes", 0, 0, 0, 0, Cube.SCREEN_WIDTH, Cube.SCREEN_HEIGHT, 1, 0);
                CuInCube.Paint();
            }
        }

        // Das Spiel wird fortgesetzt:
        private void OnUnpause()
        {
            // Jeder Cube bekommt die allgemeinen Funktionen wieder und das Bild der zuvor eingestellten Note wird auf dem Cube angezeigt
            foreach (Cube CuInCube in CubeSet)
            {
                W�rfel w�rfel = (W�rfel)CuInCube.userData;
                CuInCube.ShakeStartedEvent -= w�rfel.ShakePause;
                CuInCube.ButtonEvent += w�rfel.OnButton;
                CuInCube.TiltEvent += w�rfel.OnTilt;
                CuInCube.ShakeStartedEvent += w�rfel.OnShakeStarted;
                CuInCube.Image(NoteBilder[w�rfel.tiltPosition], 0, 0, 0, 0, Cube.SCREEN_WIDTH, Cube.SCREEN_HEIGHT, 1, 0);
                CuInCube.Paint();
            }
        }

        // Ein weiterer Cube wird "angeschlossen":
        private void OnNewCube(Cube CuInCube)
        {
            W�rfel w�rfel = (W�rfel)CuInCube.userData;
            if (w�rfel == null)
            {
                // Der neue Cube wird genauso definiert wie die Cubes in "Setup"
                w�rfel = new W�rfel(this, CuInCube);
                w�rfel.mNote = Note;
                
                // Die Arrays werden vergr��ert, da eine l�ngere Tonfolge m�glich ist
                Array.Resize(ref randomTonfolge, randomTonfolge.Length + 1);
                Array.Resize(ref randomTonfolgeAbfrage, randomTonfolgeAbfrage.Length + 1);
            }
        }

        // Ein Cube geht "verloren"/ist au�er Reichweite:
        private void OnLostCube(Cube CuInCube)
        {
            W�rfel w�rfel = (W�rfel)CuInCube.userData;
            if (w�rfel != null)
            {
                // Die Daten auf dem Cube werden gel�scht
                CuInCube.userData = null;

                // Die Arrays werden verkleinert, da Anzahl der Cubes kleiner geworden ist
                Array.Resize(ref randomTonfolge, randomTonfolge.Length - 1);
                Array.Resize(ref randomTonfolgeAbfrage, randomTonfolgeAbfrage.Length - 1);
            }
        }

        override public void Tick()
        {
            foreach (Cube cube in CubeSet)
            {
                W�rfel w�rfel = (W�rfel)cube.userData;
                // Wenn der Cube nach oben/unten geneigt wird, soll die Methode "OnTilt" automatisch abgerufen werden.
                if (cube.Tilt[Cube.TILT_Y] == 2)
                {
                    w�rfel.OnTilt(cube, 1, 1, 1);
                    w�rfel.OnTilt(cube, 1, 2, 1);
                }
                else if (cube.Tilt[Cube.TILT_Y] == 0)
                {
                    w�rfel.OnTilt(cube, 1, 1, 1);
                    w�rfel.OnTilt(cube, 1, 0, 1);
                }
            }
        }

        // development mode only
        // start CubeInstrument as an executable and run it, waiting for Siftrunner to connect

        static void Main(string[] args)
        {
            new CubeInstrument().Run();
        }
    }
}