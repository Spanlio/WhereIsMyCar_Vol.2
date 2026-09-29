# Where Is My Car? Vol. 2

2D Unity spēle, kurā spēlētājam jāatrod pareizā vieta katram transportlīdzeklim un jānovieto tas atbilstošajā vietā pilsētā.

## Projekta ideja

Spēles laukums attēlo pilsētu ar dažādām vietām, kurās iespējams novietot transportlīdzekļus.

Spēlētājam ir pieejami 12 dažādi transportlīdzekļi un 12 tiem atbilstoši galamērķa objekti. Katram transportlīdzeklim un tā galamērķim ir vienāds tags, pēc kura spēle pārbauda, vai transportlīdzeklis ir novietots pareizajā vietā.

Lai spēle katrā reizē būtu citādāka, transportlīdzekļu un galamērķu sākuma pozīcijas tiek randomizētas.

Transportlīdzekļiem ir 17 iespējamie spawn punkti, no kuriem katras spēles sākumā tiek izvēlēti 12. Galamērķiem ir vēl 17 pilnīgi atsevišķi spawn punkti, no kuriem arī tiek izvēlēti 12.

## Ekrānšāviņi

### Sākuma ekrāns

![Sākuma ekrāns](Screenshots/start-scene.png)

### Spēles laukums

![Spēles laukums](Screenshots/gameplay.png)

### Spēles beigu panelis

![Spēles beigu panelis](Screenshots/end-game.png)

> Piezīme: ekrānšāviņi jāievieto repozitorija `Screenshots` mapē ar nosaukumiem `start-scene.png`, `gameplay.png` un `end-game.png`.

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
- Nejauša horizontāla transportlīdzekļu un galamērķu apgriešana
- Pareizas un nepareizas vietas pārbaude
- Dzīvības sistēma
- Bombas un lidojošie objekti
- Iznīcinātu transportlīdzekļu uzskaite
- Zvaigžņu vērtējums
- DNF beigu nosacījums
- Uzvaras un zaudējuma beigu panelis
- Spēles atkārtota sākšana un atgriešanās uz sākuma izvēlni
- Skaņas efekti
- Neliels nejaušs rotācijas leņķis katras spēles sākumā

## Spēles beigu nosacījumi

Spēlē tiek izmantotas 3 dzīvības. Sadursmes ar bumbām, mākoņiem un lidmašīnām var samazināt dzīvību un iznīcināt pašlaik pārvietoto transportlīdzekli.

Transportlīdzekļu iznīcināšana ietekmē arī gala vērtējumu:

- 0 iznīcināti transportlīdzekļi — iespējamas līdz 3 zvaigznēm
- 1 iznīcināts transportlīdzeklis — maksimums 2 zvaigznes
- 2 vai 3 iznīcināti transportlīdzekļi — maksimums 1 zvaigzne
- 4 iznīcināti transportlīdzekļi — DNF

Ja visi atlikušie transportlīdzekļi ir novietoti pareizajās vietās, spēlētājs uzvar.

## Kā darbojas randomizācija

Spēles sākumā tiek veikti divi neatkarīgi randomizācijas procesi.

### Transportlīdzekļi

No 17 `CarsSpawnPoints` punktiem nejauši tiek izvēlēti 12. Katrs no 12 transportlīdzekļiem tiek novietots savā punktā.

Vienā spēles reizē viens spawn punkts nevar tikt izmantots vairāk nekā vienam transportlīdzeklim.

Katram transportlīdzeklim ir 50% iespēja tikt horizontāli apgrieztam.

### Galamērķi

No 17 `PlaceSpawnPoints` punktiem neatkarīgi tiek izvēlēti 12. Katrs no 12 baltajiem galamērķa objektiem tiek novietots savā punktā.

Vienā spēles reizē viens galamērķa spawn punkts nevar tikt izmantots vairāk nekā vienam objektam.

Katram galamērķim ir 50% iespēja tikt horizontāli apgrieztam.

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

Pārvalda galvenos spēles objektus, spēles stāvokli, dzīvības, uzvaras/zaudējuma nosacījumus, transportlīdzekļu iznīcināšanu un to nejaušo izvietošanu spēles sākumā.

### `DragAndDropScript`

Atbild par transportlīdzekļu izvēli un pārvietošanu ar peli.

### `DropPlaceScript`

Pārbauda, vai transportlīdzeklis ir novietots pareizajā vietā. Pārbaude tiek veikta, salīdzinot tagu, rotāciju un izmēru.

### `ObjectTransformationScript`

Atbild par transportlīdzekļa rotāciju, izmēra maiņu un horizontālo apgriešanu.

### `FlyingObjectControllerScript`

Pārvalda lidojošos objektus, bumbas, sadursmes ar transportlīdzekļiem un bumbu sprādziena animāciju.

### `EndGamePanelScript`

Pārvalda uzvaras, zaudējuma un DNF beigu paneli, zvaigžņu attēlojumu, atkārtotu spēles sākšanu un atgriešanos uz sākuma izvēlni.

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

- [x] Pabeigt un pārbaudīt randomizācijas sistēmu
- [x] Pārbaudīt visus 12 transportlīdzekļus
- [x] Pārbaudīt visus 12 galamērķa objektus
- [x] Pārbaudīt, ka viens spawn punkts netiek izmantots vairākas reizes
- [x] Pārbaudīt, ka transportlīdzeklis nepareizas novietošanas gadījumā atgriežas savā nejauši izvēlētajā sākuma pozīcijā
- [ ] Pārbaudīt visus skaņas efektus
- [x] Uzlabot pareizas novietošanas pārbaudi
- [ ] Pārbaudīt spēli uz dažādām ekrāna izšķirtspējām
- [ ] Veikt gala testēšanu
- [x] Novērst galvenās spēles kļūdas
- [x] Pievienot uzvaras un zaudējuma paneli
- [x] Pievienot zvaigžņu vērtējumu
- [x] Pievienot DNF nosacījumu
- [x] Pievienot iespēju atkārtot spēli un atgriezties sākuma izvēlnē

## Projekta statuss

Projekts atrodas izstrādes noslēguma posmā. Spēles galvenā mehānika, transportlīdzekļu pārvietošana, objektu pārveidošana, pareizās vietas pārbaude, nejaušā objektu izvietošana, dzīvību sistēma un spēles beigu nosacījumi ir izstrādāti.

## Autors

Daniels Ēriks Markovs
