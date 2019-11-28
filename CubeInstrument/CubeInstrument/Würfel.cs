using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sifteo;

namespace CubeInstrument
{
    public class Würfel
    {
        // Diese Arrays und Variablen werden für jeden Cube einzelnd benötigt:
        
        public CubeInstrument mApp;        
        public Cube mCube;
        public Sound[] mNote;
        
        public int tiltPosition;
        // Integer, der für die Bestimmung der Note zuständig ist
        
        public int tiltEventY = 1;
        // Integer, der dafür sorgt, dass das Tilt-Event beim Neigen nach oben/unten aktiviert wird.

        // Methode, die den Cubes alle "eigenen" Variablen und Funktionen übergibt 
        public Würfel(CubeInstrument app, Cube cube)
        {
            mCube = cube;
            mCube.userData = this;            
            mApp = app;

            tiltPosition = 14;
            // Cube zeigt bei Start den Ton "c1" an

            mCube.ButtonEvent += OnButton;
            mCube.TiltEvent += OnTilt;
            mCube.ShakeStartedEvent += OnShakeStarted;
            // Methodenzuordnung, die bei Interaktion mit einem Cube passieren soll

            mCube.Image(mApp.NoteBilder[tiltPosition], 0, 0, 0, 0, Cube.SCREEN_WIDTH, Cube.SCREEN_HEIGHT, 1, 0);
            mCube.Paint();
            // Abbilden der Note als Bild auf dem Cube-Display
        }

        // Methode, die bei Drücken auf einen Cube aufgerufen wird
        public void OnButton(Cube cube, bool pressed)
        {
            if (pressed)
            {
                // Wenn Cube keine Nachbarn hat, soll nur die Note auf dem gedrückten Cube abgespielt werden
                if (cube.Neighbors.IsEmpty == true)
                {
                    Ton(mNote[tiltPosition]);
                }
                // Wenn auf den ersten Cube einer gebildeten Cube-Reihe gedrückt wird, soll Tonfolge abgespielt werden
                // BugFix: Button kann nicht bei einem Cube mit linkem Nachbar gedrückt werden
                if (cube.Neighbors.Left == null && cube.Neighbors.Right != null)
                {
                    Tonfolge(mNote[tiltPosition]);
                    foreach (Cube CuInCube in mApp.CubeSet)
                    {
                        //BugFix: Wenn Tonfolge gespielt wird, soll man die Cubes nicht schütteln können
                        Würfel würfel = (Würfel)CuInCube.userData;
                        CuInCube.ShakeStartedEvent -= würfel.OnShakeStarted;
                    }
                }
            }
        }

        // Methode zum Abspielen eines einzelnen Sounds
        private void Ton(Sound sound)
        {
            // Überprüfung, ob Sound abgespielt wird; wenn Sound schon abgespielt wird, soll dieser nicht nochmals abgespielt werden
            // Aus diesem Grund bekommt jeder Cube ein NoteArray: damit es möglich ist, das zwei Cubes die gleiche Note gleichzeitig abspielen können
            if (sound.IsPlaying == false)
            {
                sound.Play(1, 0);
            }
        }

        // Methode zum Abspielen einer Tonfolge
        private void Tonfolge(Sound sound)
        {
            Ton(sound);
            // Erst wird der Sound abgespielt, dann wird überprüft, ob ein weiterer Sound vorhanden ist

            if (mApp.SpielLäuft && (mApp.randomTonfolgePosition >= mApp.randomTonfolgeAbfrage.Length - 1 || mCube.Neighbors.Right == null))
            {
                // Wenn das Tonfolge-Abfrage-Spiel läuft und kein Nachbar vorhanden ist oder die Maximallänge des AbfrageArrays erreicht ist:
                
                Log.Debug("HI");
                // Variante 1, durch Performation des Programms nicht mehr gebraucht
                /*foreach (Cube CuInCube in mApp.CubeSet)
                {
                    Würfel würfel = (Würfel)CuInCube.userData;
                    würfel.randomTonfolgeAbfrage[randomTonfolgePosition] = sound;
                }
                sound.StoppedEvent += soundStoppedTonfolgeVergleich;*/

                // Variante 2, geschrieben wegen der Performation:

                mApp.randomTonfolgeAbfrage[mApp.randomTonfolgePosition] = sound;
                // Sound wird auf letzter ArrayPosition gespeichert, s. Log.Debug:
                Log.Debug("Sound auf Position " + mApp.randomTonfolgePosition + " gelegt.");
                sound.StoppedEvent += soundStoppedTonfolgeVergleich;
                // Nach Stoppen des letzten Sounds wird die Abfrage gestartet
            }
            else if (mApp.SpielLäuft == false && mCube.Neighbors.Right == null)
            {
                // Wenn kein Tonfolge-Abfrage-Spiel stattfindet und Cube keinen rechten Nachbarn erkennt:
                foreach (Cube CuInCube in mApp.CubeSet)
                {
                    // Jeder Cube bekommt das ShakeStartEvent wieder übergeben
                    Würfel würfel = (Würfel)CuInCube.userData;
                    CuInCube.ShakeStartedEvent += würfel.OnShakeStarted;
                }
            }
            else if (mCube.Neighbors.Right != null)
            {
                // Wenn Cube einen rechten Nachbarn erkennt:
                Log.Debug("HA");
                sound.StoppedEvent += soundStoppedTonfolge;
                // Wenn Sound stoppt, soll Programm mit der SoundStop-Methode auf den rechten Cube zugreifen

                if (mApp.SpielLäuft)
                {
                    // Wenn ein Tonfolge-Abfrage Spiel stattfindet:
                    
                    // Variante 1, durch Performation des Programms nicht mehr gebraucht
                    /*foreach (Cube CuInCube in mApp.CubeSet)
                    {
                        Würfel würfel = (Würfel)CuInCube.userData;
                        Log.Debug(""+ randomTonfolgePosition);
                        würfel.randomTonfolgeAbfrage[würfel.randomTonfolgePosition] = sound;
                        Log.Debug("Sound auf Position " + randomTonfolgePosition + "gelegt.");
                        würfel.randomTonfolgePosition++;
                    }*/

                    // Variante 2, geschrieben wegen der Performation:
                    mApp.randomTonfolgeAbfrage[mApp.randomTonfolgePosition] = sound;
                    // s. Log.Debug:
                    Log.Debug("Sound auf Position " + mApp.randomTonfolgePosition + " gelegt.");
                    
                    mApp.randomTonfolgePosition++;
                    // randomTonfolgePosition wird hochgezählt, um nacheinander das Array zu befüllen
                }
            }
        }

        // Methode, die, wenn vorhanden, die nächste Note aufruft
        private void soundStoppedTonfolge(Sound sound)
        {
            sound.StoppedEvent -= soundStoppedTonfolge;
            // BugFix: SoundStopEvent wird aufgrund von möglicher Doppelung bei Neuaufruf wieder weggenommen

            // BugFix: Wenn kein rechter Nachbar vorhanden sein sollte (ob allgemein oder bei einem Loop abgetrennt), wird die Tonfolge gestoppt
            if (mCube.Neighbors.Right != null)
            {
                // Nächster Ton in der Cub-Reihe soll abgespielt werden
                Würfel würfel = (Würfel)mCube.Neighbors.Right.userData;
                würfel.Tonfolge(mNote[würfel.tiltPosition]);
            }
        }

        // Methode, die, wenn Tonfolge-Abfrage-Spiel läuft, den Vergleich aufruft und bei richtiger/falscher Antwort reagiert
        private void soundStoppedTonfolgeVergleich(Sound sound)
        {
            sound.StoppedEvent -= soundStoppedTonfolgeVergleich;
            // BugFix: SoundStopEvent wird aufgrund von möglicher Doppelung bei Neuaufruf wieder weggenommen

            Log.Debug("soundStoppedTonfolgenVergleich erreicht");
            if (TonfolgeVergleich(mApp.randomTonfolgeAbfrage, mApp.randomTonfolge))
            {
                // Wenn Tonfolge richtig: positives Ereignis
                Log.Debug("Tonfolge richtig erstellt, Spiel zu Ende");
                mApp.SpielSounds[1].Play(1, 0);
                
                mApp.Schwierigkeitsstufe++; 
                Log.Debug("" + mApp.Schwierigkeitsstufe);
                
                // Alle Tonfolge-Abfrage-Spieldaten außer der Schwierigkeitsstufe werden zurückgesetzt
                mApp.SpielLäuft = false;
                mApp.randomTonfolgePosition = 0;
                Array.Clear(mApp.randomTonfolge, 0, mApp.randomTonfolge.Length);
                Array.Clear(mApp.randomTonfolgeAbfrage, 0, mApp.randomTonfolgeAbfrage.Length);

                foreach (Cube CuInCube in mApp.CubeSet)
                {
                    // Jeder Cube bekommt das ShakeStartEvent wieder übergeben
                    Würfel würfel = (Würfel)CuInCube.userData;
                    CuInCube.ShakeStartedEvent += würfel.OnShakeStarted;
                }
            }
            else
            {
                // Wenn Tonfolge falsch: negatives Ereignis
                Log.Debug("Tonfolge falsch erstellt, versuch es nochmal");
                if (mApp.randomTonfolgePosition >= mApp.randomTonfolgeAbfrage.Length - 1)
                {
                    // Wenn erstellte Tonfolge die gleiche Länge wie die zufällige Tonfolge hat:
                    // Sound abspielen
                    mApp.SpielSounds[0].Play(1, 0);
                }

                // Daten werden gelöscht/zurückgesetzt, um weitere Abfrage zu ermöglichen
                mApp.randomTonfolgePosition = 0;
                Array.Clear(mApp.randomTonfolgeAbfrage, 0, mApp.randomTonfolgeAbfrage.Length);
                
                foreach (Cube CuInCube in mApp.CubeSet)
                {
                    // Jeder Cube bekommt das ShakeStartEvent wieder übergeben
                    Würfel würfel = (Würfel)CuInCube.userData;
                    CuInCube.ShakeStartedEvent += würfel.OnShakeStarted;
                }
            }
        }

        // Methode für Array-Vergleich
        private static bool TonfolgeVergleich(Sound[] a1, Sound[] a2)
        {
            if (a1 == a2) // Abfrage: selbes Array?
                return true;
            if (a1 == null || a2 == null) // Abfrage ein/beide Arrays haben keinen Inhalt?
                return false;
            if (a1.Length != a2.Length) // Abfrage: Arrays sind unterschiedlich lang?
                return false;
            for (int i = 0; i < a1.Length; i++) // Vergleich der Arrayinhalte auf den gleichen Positionen
                if (a1[i] != a2[i])
                    return false;
            return true;
        }

        // Methode zum Ändern der Note auf einem Cube
        public void OnTilt(Cube cube, int tiltX, int tiltY, int tiltZ)
        {
            // BugFix: TiltEvent reagiert nur dann, wenn der Cube nach oben/unten geneigt wurde
            if (tiltY != tiltEventY)
            {
                // Cube wird nach unten geneigt
                if (tiltY == 0)
                {
                    if (tiltPosition >= 0 && tiltPosition < mNote.Length - 1)
                    {
                        tiltEventY = tiltY;
                        tiltPosition++;
                        mCube.Image(mApp.NoteBilder[tiltPosition], 0, 0, 0, 0, Cube.SCREEN_WIDTH, Cube.SCREEN_HEIGHT, 1, 0);
                        mCube.Paint();
                    }
                }
                // Cube ist auf "neutraler" Position
                else if (tiltY == 1)
                {
                    tiltEventY = tiltY;
                }
                // Cube wird nach oben geneigt
                else if (tiltY == 2)
                {
                    if (tiltPosition < mNote.Length && tiltPosition > 0)
                    {
                        tiltEventY = tiltY;
                        tiltPosition--;
                        mCube.Image(mApp.NoteBilder[tiltPosition], 0, 0, 0, 0, Cube.SCREEN_WIDTH, Cube.SCREEN_HEIGHT, 1, 0);
                        mCube.Paint();
                    }
                }
            }
        }

        // Methode zum Starten des Tonfolge-Abfrage-Spiels/Überprüfung, ob ein Tonfolge-Abfrage-Spiel schon stattfindet
        public void OnShakeStarted(Cube cube)
        {
            // BugFix: Wenn ein Cube geschüttelt wird, soll auch nur dieser das ShakeStopEvent aufrufen
            cube.ShakeStoppedEvent += OnShakeStopped;
            foreach (Cube CuInCube in mApp.CubeSet)
            {
                Log.Debug("" + CuInCube.IsShaking);
                Würfel würfel = (Würfel)CuInCube.userData;
                // BugFix: Wenn geschüttelt wird, soll das Tilt- und ShakeEvent solange nicht reagieren, bis
                //         die Tonfolge zu Ende gespielt hat.
                CuInCube.ShakeStartedEvent -= würfel.OnShakeStarted;
                CuInCube.TiltEvent -= würfel.OnTilt;
            }
            // Überprüfung, ob schon ein Tonfolge-Abfrage-Spiel läuft oder nicht
            if (mApp.SpielLäuft == false)
            {
                mApp.SpielLäuft = true;
                Log.Debug("" + mApp.Schwierigkeitsstufe);
                // Überprüfung, wie oft das Tonfolge-Abfrage-Spiel erfolgreich abgeschlossen wurde
                if (mApp.Schwierigkeitsstufe >= 4)
                {
                    ErstelleRandomTonfolge(0, mNote.Length);
                    // Erstellen einer Tonfolge mit Auswahl aller Noten
                }
                else
                {
                    ErstelleRandomTonfolge(7, 15);
                    // Erstellen einer Tonfolge mit einer kleinen Auswahl an Noten
                }
            }
        }

        // Methode zum Erstellen einer zufälligen Tonfolge
        private void ErstelleRandomTonfolge(int min, int max)
        {
            Random random = new Random();
            for (int i = 0; i < mApp.randomTonfolge.Length; i++)
            {
                int randomNumber = random.Next(min, max);
                mApp.randomTonfolge[i] = mNote[randomNumber];
                Log.Debug(mApp.NoteBilder[randomNumber]);
            }
        }

        // Methode, die den Aufruf zum Abspielen der zufälligen Tonfolge gibt
        private void OnShakeStopped(Cube cube, int duration)
        {
            Log.Debug("" + duration);
            cube.ShakeStoppedEvent -= OnShakeStopped;
            // BugFix: ShakeStopEvent wird aufgrund von möglicher Doppelung bei Neuaufruf wieder weggenommen

            if (mApp.randomTonfolge != null && duration < 3000)
            {
                // Wenn Cube weniger als drei Sekunden lang geschüttelt wurde: 
                // Spiele die zufällige Tonfolge ab
                Ton(mApp.randomTonfolge[mApp.randomTonfolgePosition]);
                mApp.randomTonfolge[mApp.randomTonfolgePosition].StoppedEvent += soundStoppedRandomTonfolge;
                // Sound soll, wenn zu Ende abgespielt, StoppedEvent-Methode aufrufen
            }
            else
            {
                // Wenn Cube länger als drei Sekunden lang geschüttelt wurde: 
                // Lösche das aktuelle Tonfolge-Abfrage-Spiel
                mApp.SpielLäuft = false;
                Array.Clear(mApp.randomTonfolge, 0, mApp.randomTonfolge.Length);
                if (mApp.Schwierigkeitsstufe >= 4)
                {
                    // Zurücksetzen des Integers, wenn Tonfolge-Abfrage-Spiel schon zu oft erfolgreich absolviert wurde
                    mApp.Schwierigkeitsstufe = 0;
                }
                foreach (Cube CuInCube in mApp.CubeSet)
                {
                    Würfel würfel = (Würfel)CuInCube.userData;
                    CuInCube.TiltEvent += würfel.OnTilt;
                    CuInCube.ShakeStartedEvent += würfel.OnShakeStarted;
                    // Jeder Cube bekommt das Tilt- und das ShakeStartEvent zurück
                }
            }
        }

        // Methode, die die zufällige Tonfolge abspielt
        private void soundStoppedRandomTonfolge(Sound sound)
        {
            sound.StoppedEvent -= soundStoppedRandomTonfolge;
            // BugFix: SoundStopEvent wird aufgrund von möglicher Doppelung bei Neuaufruf wieder weggenommen

            mApp.randomTonfolgePosition++;
            if (mApp.randomTonfolgePosition < mApp.randomTonfolge.Length - 1)
            {
                // "randomTonfolgePosition"er Ton der zufälligen Tonfolge wird abgespielt
                Ton(mApp.randomTonfolge[mApp.randomTonfolgePosition]);
                mApp.randomTonfolge[mApp.randomTonfolgePosition].StoppedEvent += soundStoppedRandomTonfolge;
            }
            else if (mApp.randomTonfolgePosition == mApp.randomTonfolge.Length - 1)
            {
                // Letzter Ton der zufälligen Tonfolge wird abgespielt
                Ton(mApp.randomTonfolge[mApp.randomTonfolgePosition]);
                mApp.randomTonfolgePosition = 0;
                Log.Debug("randomTonfolgePosition wird zurückgesetzt." + mApp.randomTonfolgePosition);

                foreach (Cube CuInCube in mApp.CubeSet)
                {
                    Würfel würfel = (Würfel)CuInCube.userData;
                    CuInCube.TiltEvent += würfel.OnTilt;
                    CuInCube.ShakeStartedEvent += würfel.OnShakeStarted;
                    // Jeder Cube bekommt das Tilt- und das ShakeStartEvent zurück
                }
            }
        }

        // Lösch-Funktion des Tonfolge-Abfrage-Spiels, welches bei Pausieren des Spiels durch Schütteln ausgeführt wird
        public void ShakePause(Cube cube)
        {
            mApp.SpielLäuft = false;
            mApp.Schwierigkeitsstufe = 0;
            mApp.randomTonfolgePosition = 0;
            Array.Clear(mApp.randomTonfolge, 0, mApp.randomTonfolge.Length);
            Array.Clear(mApp.randomTonfolgeAbfrage, 0, mApp.randomTonfolgeAbfrage.Length);
        }
    }
}