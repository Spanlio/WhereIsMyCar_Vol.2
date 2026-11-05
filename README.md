# Where Is My Car? Vol. 2

2D Unity spēle, kurā spēlētājam jāatrod pareizā vieta katram transportlīdzeklim un jānovieto tas atbilstošajā vietā pilsētā.

Sākuma izvēlnē var izvēlēties vienu no divām spēlēm: **Where Is My Car?** (Pilsētas aina) vai **Hanojas tornis** (Hanojas aina).

## Projekta ideja

Spēles laukums attēlo pilsētu ar dažādām vietām, kurās iespējams novietot transportlīdzekļus.

Spēlētājam ir pieejami 12 dažādi transportlīdzekļi un 12 tiem atbilstoši galamērķa objekti. Katram transportlīdzeklim un tā galamērķim ir vienāds tags, pēc kura spēle pārbauda, vai transportlīdzeklis ir novietots pareizajā vietā.

Lai spēle katrā reizē būtu citādāka, transportlīdzekļu un galamērķu sākuma pozīcijas tiek randomizētas.

Transportlīdzekļiem ir 17 iespējamie spawn punkti, no kuriem katras spēles sākumā tiek izvēlēti 12. Galamērķiem ir vēl 17 pilnīgi atsevišķi spawn punkti, no kuriem arī tiek izvēlēti 12.

## Pilsētas aina

<!-- Šeit pievienot Pilsētas ainas attēlus -->

## Transportlīdzekļi

Spēlē ir 12 transportlīdzekļi:

- Atkritumu mašīna
- Ātrā palīdzība
- Skolas autobuss
- B2
- Cementa mašīna
- BMW E46
- BMW E61
- Ekskavators
- Policija
- Traktors 1
- Traktors 5
- Ugunsdzēsēju mašīna

## Galvenās iespējas

- 12 dažādi transportlīdzekļi
- 12 atbilstoši galamērķa objekti
- 17 iespējami transportlīdzekļu spawn punkti
- 17 atsevišķi galamērķu spawn punkti
- Nejauša transportlīdzekļu izvietošana
- Nejauša galamērķu izvietošana
- Transportlīdzekļu pārvietošana ar peli
- Transportlīdzekļu pagriešana
- Transportlīdzekļu izmēra maiņa
- Horizontāla apgriešana
- Pareizas un nepareizas vietas pārbaude
- Skaņas efekti
- Neliels nejaušs rotācijas leņķis katras spēles sākumā

## Kā darbojas randomizācija

Spēles sākumā tiek veikti divi neatkarīgi randomizācijas procesi.

### Transportlīdzekļi

No 17 `CarsSpawnPoints` punktiem nejauši tiek izvēlēti 12. Katrs no 12 transportlīdzekļiem tiek novietots savā punktā.

Vienā spēles reizē viens spawn punkts nevar tikt izmantots vairāk nekā vienam transportlīdzeklim.

### Galamērķi

No 17 `PlaceSpawnPoints` punktiem neatkarīgi tiek izvēlēti 12. Katrs no 12 baltajiem galamērķa objektiem tiek novietots savā punktā.

Vienā spēles reizē viens galamērķa spawn punkts nevar tikt izmantots vairāk nekā vienam objektam.

Transportlīdzekļa un galamērķa savstarpējā saistība netiek randomizēta. To nosaka to tagi.

Piemēram:

```text
Policija
Tag: Policija

PolicijaVieta
Tag: Policija
```

Tas nozīmē, ka `Policija` transportlīdzeklis ir paredzēts `PolicijaVieta` objektam neatkarīgi no tā, kur abi objekti atrodas konkrētajā spēles reizē.

## Vadība

| Taustiņš / poga | Darbība |
|---|---|
| Kreisā peles poga | Izvēlēties un pārvietot transportlīdzekli |
| `Z` | Pagriezt pa kreisi |
| `X` | Pagriezt pa labi |
| `↑` | Palielināt augstumu |
| `↓` | Samazināt augstumu |
| `←` | Samazināt platumu |
| `→` | Palielināt platumu |
| `Space` | Apgriezt horizontāli |
| Poga `Izvēlne` | Atgriezties sākuma izvēlnē |

## Hanojas aina

Hanojas tornis ir loģikas spēle ar trim stabiem. Spēles sākumā visi diski atrodas uz kreisā staba, un tie jāpārvieto uz labo stabu. Vienā gājienā drīkst pārvietot vienu disku, un lielāku disku nedrīkst likt uz mazāka. Nepareizs gājiens netiek ieskaitīts. Aina vienmēr tiek rādīta horizontāli.

<!-- Šeit pievienot Hanojas ainas attēlus -->

### Spēles gaita

Spēles sākumā jāizvēlas disku skaits: 3, 4 vai 5. Disku var vilkt ar pirkstu vai peli. Var arī pieskarties stabam, lai paceltu tā augšējo disku, un pēc tam citam stabam, lai disku tur noliktu.

Spēles laikā tiek skaitīts laiks un gājieni. Pēc uzvaras var spēlēt vēlreiz, pāriet uz lielāku disku skaitu vai izvēlēties disku skaitu no jauna.

### Zvaigznes

Rezultātu vērtē ar 1 līdz 3 zvaigznēm. Lai iegūtu 2 vai 3 zvaigznes, jāizpilda gan gājienu, gan laika nosacījums.

| Diski | ★★★ gājieni / laiks | ★★ gājieni / laiks | ★ |
|---|---|---|---|
| 3 | 7 / līdz 21 s | līdz 10 / līdz 42 s | Tornis pārvietots |
| 4 | 15 / līdz 45 s | līdz 22 / līdz 90 s | Tornis pārvietots |
| 5 | 31 / līdz 93 s | līdz 46 / līdz 186 s | Tornis pārvietots |

Uzvaras ekrānā vienu reizi spēlē var noskatīties reklāmu, kas samazina rezultāta laiku uz pusi. Ja ar jauno laiku pienākas vairāk zvaigžņu, tiek pieskaitītas tikai papildu zvaigznes.

Iegūtās zvaigznes uzkrājas kopā ar Pilsētas ainā nopelnītajām, un tās var tērēt sākuma izvēlnes veikalā, piemēram, Hanojas ainas debesu tēmām.

## Tehnoloģijas

- C#
- Unity 6
- Unity UI
- Unity Event System

## Izmantotie skripti

### `GameObjectsScript`

Pārvalda galvenos spēles objektus un to nejaušo izvietošanu spēles sākumā.

### `DragAndDropScript`

Atbild par transportlīdzekļu izvēli un pārvietošanu ar peli.

### `DropPlaceScript`

Pārbauda, vai transportlīdzeklis ir novietots pareizajā vietā. Pārbaude tiek veikta, salīdzinot tagu, rotāciju un izmēru.

### `ObjectTransformationScript`

Atbild par transportlīdzekļa rotāciju, izmēra maiņu un horizontālo apgriešanu.

### `ScreenBoundariesScript`

Ierobežo transportlīdzekļu pārvietošanu spēles laukuma robežās.

### `CameraZoomScript`

Pārvalda kameras tuvināšanu un tālināšanu.

### `HanoiGameScript`

Pārvalda Hanojas torņa spēli: gājienus, laiku, zvaigznes, uzvaras ekrānu un debesu tēmas.

### `HanoiDiskScript`

Atbild par diska pieskārienu un vilkšanu.

### `HanoiPoleScript`

Uztver pieskārienu stabam.

### `HanoiBoardFitScript`

Pielāgo spēles laukumu ekrāna izmēram.

### `LandscapeOnlyScript`

Hanojas ainā nofiksē horizontālu ekrāna orientāciju un, izejot no ainas, atļauj to atkal mainīt.

## Projekta struktūra

```text
Canvas
├── Cars
│   ├── GarbageTruck
│   ├── Medicine
│   ├── SchoolBus
│   ├── b2
│   ├── CementaMasina
│   ├── e46
│   ├── e61
│   ├── Ekskavators
│   ├── Policija
│   ├── Traktors1
│   ├── Traktors5
│   └── Ugunsdzeseji
│
├── Places
│   ├── BackgroundImage
│   ├── GarbageTruckPlace
│   ├── AmbulancePlace
│   ├── SchoolBusPlace
│   ├── b2Vieta
│   ├── CementaMasinaVieta
│   ├── e46Vieta
│   ├── e61Vieta
│   ├── EkskavatorsVieta
│   ├── PolicijaVieta
│   ├── Traktors1Vieta
│   ├── Traktors5Vieta
│   └── UgunsdzesejiVieta
│
├── CarsSpawnPoints
│   ├── CarsPlace1
│   ├── CarsPlace2
│   ├── ...
│   └── CarsPlace17
│
└── PlaceSpawnPoints
    ├── CarDestSpawn
    ├── CarDestSpawn (1)
    ├── ...
    └── CarDestSpawn (16)
```

## TODO

- [ ] Pabeigt un pārbaudīt randomizācijas sistēmu
- [ ] Pārbaudīt visus 12 transportlīdzekļus
- [ ] Pārbaudīt visus 12 galamērķa objektus
- [ ] Pārbaudīt, ka viens spawn punkts netiek izmantots vairākas reizes
- [ ] Pārbaudīt, ka transportlīdzeklis nepareizas novietošanas gadījumā atgriežas savā nejauši izvēlētajā sākuma pozīcijā
- [ ] Pārbaudīt visus skaņas efektus
- [ ] Uzlabot pareizas novietošanas pārbaudi
- [ ] Pārbaudīt spēli uz dažādām ekrāna izšķirtspējām
- [ ] Veikt gala testēšanu
- [ ] Novērst atlikušās kļūdas

## Projekta statuss

Projekta jaunākā versija ir pieejama zarā `releases` kā Android APK fails `APK/WhereIsMyCar.apk`.

Spēle ir spēlējama. Sākuma izvēlnē var izvēlēties Pilsētas ainu vai Hanojas ainu, un no abām ainām var atgriezties izvēlnē. Abās spēlēs nopelnītās zvaigznes var tērēt veikalā.

Reklāmas darbojas Unity Ads testa režīmā. Nākamais solis ir gala testēšana, kas aprakstīta sadaļā TODO.

## Autors

Daniels Ēriks Markovs
