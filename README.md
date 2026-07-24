# Shadow Zoo

Program interaktif berbasis **computer vision** yang mendeteksi bayangan tangan (shadow puppet) melalui input kamera, lalu menampilkan animasi hewan yang sesuai secara real-time.

## Cara Bermain

1. Siapkan ruangan **gelap**.
2. Gunakan **1 senter** untuk menyorot tangan dari belakang, sehingga terbentuk bayangan di dinding/layar.
3. Bentuk tangan Anda menjadi salah satu siluet hewan berikut:
   - Deer
   - Moose
   - Bird
   - Elephant
   - Panther
4. Kamera akan menangkap bentuk bayangan tersebut dan model CV akan mengklasifikasikannya.
5. Jika terdeteksi sesuai, hewan yang bersangkutan akan muncul di layar kedua.

## Tampilan Program

Program ini menggunakan **2 display**:

| Display | Fungsi |
|---|---|
| **Display 1** | Menampilkan background/panduan cara bermain (instruksi, contoh bentuk tangan, dsb.) |
| **Display 2** | Akan menampilkan hewan yang sesuai (prefab hewan di-instantiate sesuai shadow yang terdeteksi), dengan setiap hewan memiliki behaviour yang berbeda |

## Teknologi

- **Input**: Kamera (webcam) menangkap bayangan tangan secara real-time.
- **Computer Vision Model**: Diadaptasi dari [HaSPeR (Hand Shadow Puppet Recognition)](https://github.com/Starscream-11813/HaSPeR).
- **Klasifikasi hewan**: Saat ini terbatas pada 5 kelas — deer, moose, bird, elephant, panther.
- **AI Assistance**: Sebagian base game logic dan proses optimisasi kode dibantu menggunakan AI.

## Batasan Saat Ini

- Hanya mendukung 5 jenis hewan.
- Performa deteksi terbaik dicapai di ruangan gelap dengan pencahayaan senter tunggal (kontras bayangan tinggi).
- Membutuhkan kamera yang dapat menangkap kontur bayangan dengan jelas.
- Hewan belum memiliki animasi

## Kredit

- Model dasar computer vision: [Starscream-11813/HaSPeR](https://github.com/Starscream-11813/HaSPeR)
- Pengembangan game & optimisasi: dibantu AI
