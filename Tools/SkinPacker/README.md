# Skin Packer

Skin Packer stays with you. A client build is the game exe plus one `skin.pack`. The game ignores loose images. A pack that was not signed by your private key is ignored, and the built-in art stays.

## First time

1. Run `SkinPacker.exe` once from this folder (or `dotnet run --project Tools/SkinPacker`).
2. It writes `Tools/SkinPacker/keys/private-key.json` and the matching public key into `Assets/Scripts/Skin/SkinPublicKey.cs`.
3. Back up the `keys` folder somewhere that is not the client build. If that file is lost, you cannot sign a pack this game will accept.
4. Build the Windows player in Unity. Standalone is set to IL2CPP so the public key is not sitting in a C# DLL a client can replace.

## Each client

1. Open Skin Packer.
2. Set the two text colors, the wall color, and the five ghost colors.
3. Pick the client logo, pellet, four power pellets, and one Pac-Man image (PNG or JPG). Leave a file blank to keep the art already in the game. The logo is applied only to the object named Client Logo. BoothClub Logo is left alone.
4. Pac-Man should be one filled shape facing right, with no mouth. The game cuts the chomp frames from it, uses the open-mouth frame as a still image on LivesIndicator, and shrinks that same image for the death sequence.
5. Export `skin.pack` and put it in the same folder as `Pacman.exe`.

In the editor, Play mode reads `skin.pack` from the project folder (next to `Assets`).

## What not to ship

Do not send `SkinPacker.exe`, this folder, or `keys` with a client build. Do not send the Unity project.
