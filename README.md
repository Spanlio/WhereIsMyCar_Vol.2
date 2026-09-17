# Where Is My Car? Vol. 2

2D Unity spēle, kurā spēlētājam jāatrod pareizā vieta katram transportlīdzeklim un jānovieto tas atbilstošajā vietā pilsētā.

## Projekta ideja

Spēles laukums attēlo pilsētu ar dažādām vietām, kurās iespējams novietot transportlīdzekļus.

Spēlētājam ir pieejami 12 dažādi transportlīdzekļi un 12 tiem atbilstoši galamērķa objekti. Katram transportlīdzeklim un tā galamērķim ir vienāds tags, pēc kura spēle pārbauda, vai transportlīdzeklis ir novietots pareizajā vietā.

Lai spēle katrā reizē būtu citādāka, transportlīdzekļu un galamērķu sākuma pozīcijas tiek randomizētas.

Transportlīdzekļiem ir 17 iespējamie spawn punkti, no kuriem katras spēles sākumā tiek izvēlēti 12. Galamērķiem ir vēl 17 pilnīgi atsevišķi spawn punkti, no kuriem arī tiek izvēlēti 12.

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

## Ekrānšāviņi

### Spēles laukums

![Spēles laukums](docs/screenshots/gameplay.png)

### Unity Hierarchy

![Unity Hierarchy](docs/screenshots/unity-hierarchy.png)

### Unity Inspector

![Unity Inspector](docs/screenshots/unity-inspector.png)

## Projekta statuss

Projekts atrodas izstrādes procesā. Spēles galvenā mehānika, transportlīdzekļu pārvietošana, objektu pārveidošana, pareizās vietas pārbaude un nejaušā objektu izvietošana tiek izstrādāta vienotā sistēmā.

## Autors

Daniels Ēriks Markovs
