using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;

namespace Bankamatik_Simülasyonu
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SqlConnection baglanti = new SqlConnection("Data Source=KOELSYS\\SQLEXPRESS;Initial Catalog=BankamatikSimülasyonu;Integrated Security=True;");

            Console.WriteLine("Yıldız Bankasına Hoşgeldiniz. 1 = Giriş Yap, 2 = Kayıt Ol");
            int secim1 = int.Parse(Console.ReadLine());
            if(secim1 == 1)
            {
                Console.WriteLine("Hesap Numaranızı Giriniz");
                int hesapno = int.Parse(Console.ReadLine());

                Console.WriteLine("Şifrenizi Giriniz.");
                string sifre = Console.ReadLine();

                DateTime zaman = DateTime.Now;
                try
                {
                    baglanti.Open();

                    SqlCommand komut = new SqlCommand("SELECT sifre FROM Bilgiler WHERE sifre=@sifre", baglanti);
                    komut.Parameters.AddWithValue("@sifre", sifre);

                    SqlDataReader reader = komut.ExecuteReader();

                    if (reader.Read())
                    {

                        reader.Close();

                        SqlCommand komut1 = new SqlCommand("SELECT ad FROM Bilgiler WHERE hesap=@hesapno", baglanti);
                        komut1.Parameters.AddWithValue("@hesapno", hesapno);
                        string ad = komut1.ExecuteScalar() as string;

                        SqlCommand komut2 = new SqlCommand("SELECT soyad FROM Bilgiler WHERE hesap=@hesapno AND sifre = @sifre", baglanti);
                        komut2.Parameters.AddWithValue("@hesapno", hesapno);
                        komut2.Parameters.AddWithValue("@sifre", sifre);
                        string soyad = komut2.ExecuteScalar() as string;

                        if (!string.IsNullOrEmpty(ad) && !string.IsNullOrEmpty(soyad))
                        {
                            Console.WriteLine("\nGiriş Başarılı!");
                            Console.WriteLine("Hoşgeldiniz, " + ad + " " + soyad + "!");
                            Console.WriteLine("Yapmak İstediğiniz Eylemi Seçiniz. 1 = Bakiyemi Görüntüle. 2 = Para Çek, 3 = Para Yatır. 4 = Para Transferi(Havale), 5 = Hareketlerim");
                            int secim = int.Parse(Console.ReadLine());

                            SqlCommand komut4 = new SqlCommand("SELECT BAKİYE FROM Bakiye WHERE hesap=@hesapno", baglanti);
                            komut4.Parameters.AddWithValue("@hesapno", hesapno);
                            object bakiye = komut4.ExecuteScalar();
                            if (secim == 1)
                            {
                                Console.WriteLine("Bakiyeniz: " + bakiye);
                            }
                            if (secim == 2)
                            {
                                Console.WriteLine("Çekmek İstediğiniz Tutarı Giriniz.");
                                int cekilen = int.Parse(Console.ReadLine());
                                if((decimal)bakiye > cekilen)
                                {
                                    bakiye = (decimal)bakiye - cekilen;
                                    SqlCommand kmtparacek = new SqlCommand("UPDATE BAKİYE SET BAKİYE = @BAKİYE FROM Bakiye WHERE hesap = @hesapno", baglanti);
                                    kmtparacek.Parameters.AddWithValue("@BAKİYE", bakiye);
                                    kmtparacek.Parameters.AddWithValue("@hesapno", hesapno);
                                    kmtparacek.ExecuteNonQuery();
                                    Console.WriteLine("İşlem Tamamlandı.");

                                    SqlCommand kmthrktçek = new SqlCommand("INSERT INTO Hareketler (ISLEM, TUTAR, ZAMAN, GONDEREN) VALUES (@ıslem,@tutar,@zaman,@gonderen)", baglanti);
                                    kmthrktçek.Parameters.AddWithValue("@ıslem", "Para Çekme");
                                    kmthrktçek.Parameters.AddWithValue("@tutar", cekilen);
                                    kmthrktçek.Parameters.AddWithValue("@zaman", zaman);
                                    kmthrktçek.Parameters.AddWithValue("@gonderen", hesapno);
                                    
                                    kmthrktçek.ExecuteNonQuery();

                                }
                                else
                                {
                                    Console.WriteLine("Çekmek İstediğiniz Tutar Bakiyenizden Fazla Olamaz.");
                                }
                                
                            }
                            if (secim == 3)
                            {
                                Console.WriteLine("Yatırmak İstediğiniz Tutarı Giriniz.");
                                int yatirilan = int.Parse(Console.ReadLine());
                                bakiye = (decimal)bakiye + yatirilan;
                                SqlCommand kmtparayatir = new SqlCommand("UPDATE BAKİYE SET BAKİYE = @BAKİYE FROM Bakiye WHERE hesap = @hesapno", baglanti);
                                kmtparayatir.Parameters.AddWithValue("@BAKİYE", bakiye);
                                kmtparayatir.Parameters.AddWithValue("@hesapno", hesapno);
                                kmtparayatir.ExecuteNonQuery();
                                Console.WriteLine("İşlem Tamamlandı.");

                                SqlCommand kmthrktyatir = new SqlCommand("INSERT INTO Hareketler (ISLEM, TUTAR, ZAMAN, GONDEREN) VALUES (@ıslem,@tutar,@zaman,@gonderen)", baglanti);
                                kmthrktyatir.Parameters.AddWithValue("@ıslem", "Para Yatırma");
                                kmthrktyatir.Parameters.AddWithValue("@tutar", yatirilan);
                                kmthrktyatir.Parameters.AddWithValue("@zaman", zaman);
                                kmthrktyatir.Parameters.AddWithValue("@gonderen", hesapno);
                                
                                kmthrktyatir.ExecuteNonQuery();
                            }
                            if(secim == 4)
                            {
                                Console.WriteLine("\nHavale Yapmak İstediğiniz Hesap Numarasını Giriniz.");
                                int alicihesapno = int.Parse(Console.ReadLine());
                                SqlCommand komut5 = new SqlCommand("SELECT AD + ' ' + SOYAD FROM Bilgiler WHERE hesap = @hesapno",baglanti);
                                komut5.Parameters.AddWithValue("@hesapno", alicihesapno);
                                string adsoyadalici = komut5.ExecuteScalar() as string;

                                Console.WriteLine("Alıcının Adını Yazınız. \nİpucu ="+adsoyadalici.Substring(0,1)+"***** "+ adsoyadalici.Split(' ')[1].Substring(0, 1) + "*****");
                                string asalici = Console.ReadLine();
                                if (asalici == adsoyadalici)
                                {
                                    Console.WriteLine(alicihesapno + " numaralı hesaba gönderilecek tutarı giriniz.");
                                    int havale = int.Parse(Console.ReadLine());
                                    if ((decimal)bakiye > havale)
                                    {
                                        bakiye = (decimal)bakiye - havale;
                                        SqlCommand kmtparacek = new SqlCommand("UPDATE BAKİYE SET BAKİYE = @BAKİYE FROM Bakiye WHERE hesap = @hesapno", baglanti);
                                        kmtparacek.Parameters.AddWithValue("@BAKİYE", bakiye);
                                        kmtparacek.Parameters.AddWithValue("@hesapno", hesapno);
                                        kmtparacek.ExecuteNonQuery();

                                        SqlCommand komut6 = new SqlCommand("SELECT BAKİYE FROM Bakiye WHERE hesap=@hesapno", baglanti);
                                        komut6.Parameters.AddWithValue("@hesapno", alicihesapno);
                                        object bakiye1 = komut6.ExecuteScalar();

                                        bakiye1 = (decimal)bakiye1 + havale;
                                        SqlCommand kmtparayatir = new SqlCommand("UPDATE BAKİYE SET BAKİYE = @BAKİYE FROM Bakiye WHERE hesap = @alicihesapno", baglanti);
                                        kmtparayatir.Parameters.AddWithValue("@BAKİYE", bakiye1);
                                        kmtparayatir.Parameters.AddWithValue("@alicihesapno", alicihesapno);
                                        kmtparayatir.ExecuteNonQuery();
                                        Console.WriteLine("İşlem Tamamlandı.");

                                        
                                        SqlCommand kmthrkthavale = new SqlCommand("INSERT INTO Hareketler (ISLEM, GONDEREN, ALICI, TUTAR, ZAMAN) VALUES (@ıslem,@gonderen,@alıcı,@tutar,@zaman)", baglanti);
                                        kmthrkthavale.Parameters.AddWithValue("@ıslem", "Havale");
                                        kmthrkthavale.Parameters.AddWithValue("@gonderen",hesapno);
                                        kmthrkthavale.Parameters.AddWithValue("@alıcı",alicihesapno);
                                        kmthrkthavale.Parameters.AddWithValue("@tutar", havale);
                                        kmthrkthavale.Parameters.AddWithValue("@zaman", zaman);
                                        kmthrkthavale.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        Console.WriteLine("Çekmek İstediğiniz Tutar Bakiyenizden Fazla Olamaz.");
                                    }
                                } 
                                else
                                {
                                    Console.WriteLine("Kişi Bulunamadı");
                                }
                            }
                            if (secim == 5)
                            {
                                SqlCommand kmthrkt = new SqlCommand("SELECT * FROM Hareketler WHERE GONDEREN = @hesapno OR ALICI = @hesap", baglanti);
                                kmthrkt.Parameters.AddWithValue("@hesapno", hesapno);
                                kmthrkt.Parameters.AddWithValue("@hesap", hesapno);
                                SqlDataReader dr = kmthrkt.ExecuteReader();
                                while (dr.Read())
                                {
                                    if (dr["ALICI"] == DBNull.Value)
                                    {
                                        Console.WriteLine($"İşlem: {dr["ISLEM"]}, Tarih: {dr["ZAMAN"]}, Miktar: {dr["TUTAR"]}");
                                    }
                                    else if (dr["ALICI"] != DBNull.Value)
                                    {
                                        Console.WriteLine($"İşlem: {dr["ISLEM"]}, Tarih: {dr["ZAMAN"]}, Miktar: {dr["TUTAR"]}, Gönderen IBAN: {dr["GONDEREN"]}, Alıcı IBAN: {dr["ALICI"]}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nGiriş Başarısız. Tekrar Deneyiniz.");

                        }
                    }
                    else
                    {
                        Console.WriteLine("Hatalı Giriş!");
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Hata: " + ex.Message);
                }
                finally
                {
                    baglanti.Close();
                }
            }

            if (secim1 == 2) 
            {
                Console.WriteLine("Adınızı Giriniz"); string ad = Console.ReadLine();
                Console.WriteLine("Soyadınızı Giriniz"); string soyad = Console.ReadLine();
                Console.WriteLine("TC Numaranızı Giriniz"); string tc = Console.ReadLine();
                Console.WriteLine("Telofon Numaranızı Giriniz"); string telefon = Console.ReadLine();
                Random random = new Random();
                int hesapno = random.Next(100000, 1000000);
                Console.WriteLine("Şifrenizi Giriniz"); string sifre = Console.ReadLine();

                try
                {
                    baglanti.Open();

                    SqlCommand kayıtkomutu = new SqlCommand("INSERT INTO Bilgiler(AD, SOYAD, TC, TELEFON, HESAP, SİFRE) VALUES (@AD, @SOYAD, @TC, @TELEFON, @HESAP, @SIFRE)", baglanti);
                    kayıtkomutu.Parameters.AddWithValue("@AD", ad);
                    kayıtkomutu.Parameters.AddWithValue("@SOYAD", soyad);
                    kayıtkomutu.Parameters.AddWithValue("@TC", tc);
                    kayıtkomutu.Parameters.AddWithValue("@TELEFON", telefon);
                    kayıtkomutu.Parameters.AddWithValue("@HESAP", hesapno);
                    kayıtkomutu.Parameters.AddWithValue("@SIFRE", sifre);

                    SqlCommand bakiyekayıt = new SqlCommand("INSERT INTO Bakiye(HESAP, BAKİYE) VALUES (@HESAP, @BAKİYE)", baglanti);
                    bakiyekayıt.Parameters.AddWithValue("@HESAP", hesapno);
                    bakiyekayıt.Parameters.AddWithValue("@BAKİYE", 0);

                    kayıtkomutu.ExecuteNonQuery();
                    bakiyekayıt.ExecuteNonQuery();

                    Console.WriteLine("Kaydınız Başarıyla Oluşturuldu.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Hata: " + ex.Message);
                }
                finally
                {
                    baglanti.Close();
                }
            }
           
            Console.Read();
        }
    }
}
