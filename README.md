# Where Is My Car? Vol. 2

2D spēle, kurā spēlētājam jāatrod pareizā vieta katram transportlīdzeklim un jānovieto tas atbilstošajā vietā pilsētā.

## Projekta ideja

Spēles laukums ir pilsētas vide ar dažādām vietām, kurās var novietot transportlīdzekļus.

Spēlētājam ir pieejami 12 dažādi transportlīdzekļi un 12 tiem atbilstoši galamērķi. Katram transportlīdzeklim un tā galamērķim ir vienāds tags, pēc kura programma nosaka, vai transportlīdzeklis ir novietots pareizajā vietā.

Lai spēle katrā reizē būtu citādāka, transportlīdzekļu un galamērķu sākuma pozīcijas tiek randomizētas.

Katram objektu veidam ir 17 iespējamie spawn punkti, no kuriem spēles sākumā tiek izvēlēti 12.

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
- Nejaušs neliels rotācijas leņķis katras spēles sākumā

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

## Kā darbojas spēle

Spēles sākumā tiek veikti divi neatkarīgi randomizācijas procesi.

### Transportlīdzekļi

No 17 `CarsSpawnPoints` punktiem nejauši tiek izvēlēti 12. Katrs no 12 transportlīdzekļiem tiek novietots savā punktā.

### Galamērķi

No 17 `PlaceSpawnPoints` punktiem nejauši tiek izvēlēti 12. Katrs no 12 galamērķa objektiem tiek novietots savā punktā.

Transportlīdzekļa un tā galamērķa saistība netiek mainīta. To nosaka objekta tags.

Piemēram:

```text
Policija
Tag: Policija

PolicijaVieta
Tag: Policija
