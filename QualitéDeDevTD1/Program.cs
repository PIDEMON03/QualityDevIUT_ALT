using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace QD
{
    public class Media
    {
        public string titre;
        public int numRef;
        public int numEx;
        public bool EstEmprunte { get; set; }

        public virtual void AfficherInfos()
        {
            Console.WriteLine("Informations générales sur le média :");
            Console.WriteLine($"Titre : {titre}");
            Console.WriteLine($"Numéro de référence : {numRef}");
            Console.WriteLine($"Numéro d'exemplaire : {numEx}");
        }
    }

    public class Livre : Media
    {
        public string auteur;

        public override void AfficherInfos()
        {
            base.AfficherInfos();
            Console.WriteLine($"Auteur : {auteur}");
        }
    }

    public class DVD : Media
    {
        public string duree;

        public override void AfficherInfos()
        {
            base.AfficherInfos();
            Console.WriteLine($"Durée : {duree} minutes");
        }
    }

    public class CD : Media
    {
        string artiste;

        public override void AfficherInfos()
        {
            base.AfficherInfos();
            Console.WriteLine($"Artiste : {artiste}");
        }
    }

    public class Emprunt
    {
        public string Utilisateur { get; set; }
        public int MediaNumeroReference { get; set; }
        public DateTime DateEmprunt { get; set; }
    }

    // Classe principale de la bibliothèque
    public class Library
    {
        private List<Media> medias = new List<Media>();
        private List<Emprunt> emprunts = new List<Emprunt>();

        // Indexeur pour accéder aux médias par numéro de référence
        public Media this[int numeroReference]
        {
            get
            {
                return medias.FirstOrDefault(media => media.numRef == numeroReference);
            }
        }

        // Méthode pour ajouter un média à la bibliothèque
        public void AjouterMedia(Media media)
        {
            medias.Add(media);
        }

        // Méthode pour retirer un média de la bibliothèque
        public void RetirerMedia(Media media)
        {
            medias.Remove(media);
        }

        // Méthode pour emprunter un média
        public void EmprunterMedia(Media media, string utilisateur)
        {
            // Recherchez le média dans la bibliothèque
            Media mediaTrouve = medias.FirstOrDefault(m => m.numRef == media.numRef);

            if (mediaTrouve == null)
            {
                throw new InvalidOperationException("Média introuvable dans la bibliothèque.");
            }

            if (mediaTrouve.numEx <= 0)
            {
                throw new EmpruntInvalideException();
            }


            emprunts.Add(new Emprunt { Utilisateur = utilisateur, MediaNumeroReference = media.numRef, DateEmprunt = DateTime.Now });
        }

        // Méthode pour retourner un média emprunté
        public void RetournerMedia(Media media)
        {
            // Recherchez le média dans la bibliothèque
            Media mediaTrouve = medias.FirstOrDefault(m => m.numRef == media.numRef);

            if (mediaTrouve == null)
            {
                throw new InvalidOperationException("Média introuvable dans la bibliothèque.");
            }

            // Vérifiez si le média a été emprunté
            Emprunt emprunt = emprunts.FirstOrDefault(e => e.MediaNumeroReference == media.numRef);

            if (emprunt == null)
            {
                throw new InvalidOperationException("Le média n'a pas été emprunté.");
            }

            // Réalisez le retour du média ici et mettez à jour les informations
            // ...

            emprunts.Remove(emprunt);
        }

        // Méthode pour rechercher des médias par critère (titre ou auteur)
        public List<Media> RechercherMedia(string critere)
        {
            critere = critere.ToLower(); // Convertir le critère en minuscules pour une recherche insensible à la casse

            List<Media> resultats = new List<Media>();

            foreach (var media in medias)
            {
                if (media.titre.ToLower().Contains(critere) || (media is Livre livre && livre.auteur.ToLower().Contains(critere)))
                {
                    resultats.Add(media);
                }
            }

            return resultats;
        }

        // Méthode pour lister les médias empruntés par un utilisateur donné
        public List<Media> ListerMediasEmpruntesParUtilisateur(string utilisateur)
        {
            return (from emprunt in emprunts
                    join media in medias on emprunt.MediaNumeroReference equals media.numRef
                    where emprunt.Utilisateur == utilisateur
                    select media).ToList();
        }

        // Méthode pour afficher les statistiques de la bibliothèque
        public void AfficherStatistiques()
        {
            int nombreTotalMedias = medias.Count;
            int nombreExemplairesEmpruntes = emprunts.Count;
            int nombreExemplairesDisponibles = medias.Sum(media => media.numEx);

            Console.WriteLine("Statistiques de la bibliothèque :");
            Console.WriteLine($"Nombre total de médias : {nombreTotalMedias}");
            Console.WriteLine($"Nombre d'exemplaires empruntés : {nombreExemplairesEmpruntes}");
            Console.WriteLine($"Nombre d'exemplaires disponibles : {nombreExemplairesDisponibles}");
        }

        // Méthode pour sauvegarder la bibliothèque dans un fichier JSON
        public void SauvegarderBibliotheque(string nomFichier)
        {
            using (FileStream fs = new FileStream(nomFichier, FileMode.Create))
            {
                JsonSerializer.Serialize(fs, this);
            }
        }

        // Méthode pour charger la bibliothèque à partir d'un fichier JSON
        public static Library ChargerBibliotheque(string nomFichier)
        {
            using (FileStream fs = new FileStream(nomFichier, FileMode.Open))
            {
                return JsonSerializer.Deserialize<Library>(fs);
            }
        }

        // Opérateur + surchargé pour ajouter un média à la bibliothèque
        public static Library operator +(Library library, Media media)
        {
            library.AjouterMedia(media);
            return library;
        }

        // Méthode pour obtenir tous les médias de la bibliothèque
        public IEnumerable<Media> ObtenirTousLesMedias()
        {
            // Utilisez LINQ pour obtenir tous les médias de la bibliothèque
            return medias;
        }
    }

    // Classe d'exception pour les emprunts invalides
    public class EmpruntInvalideException : Exception
    {
        public EmpruntInvalideException() : base("Emprunt invalide : le média n'est pas disponible.")
        {
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Créez une instance de la bibliothèque
            Library bibliothèque = new Library();

            try
            {
                // Créez et ajoutez des médias à la bibliothèque
                Livre livre1 = new Livre
                {
                    titre = "Le Seigneur des Anneaux",
                    numRef = 101,
                    numEx = 5,
                    auteur = "J.R.R. Tolkien"
                };

                DVD dvd = new DVD
                {
                    titre = "Inception",
                    numRef = 201,
                    numEx = 10,
                    duree = "2 heures 28 minutes"
                };

                // Ajoutez les médias à la bibliothèque en utilisant l'opérateur surchargé
                bibliothèque += livre1;
                bibliothèque += dvd;

                // Emprunt d'un média
                bibliothèque.EmprunterMedia(livre1, "Utilisateur1");
                Console.WriteLine("Emprunt réussi.");

                // Retour d'un média emprunté
                bibliothèque.RetournerMedia(livre1);
                Console.WriteLine("Retour réussi.");

                // Afficher les statistiques de la bibliothèque
                bibliothèque.AfficherStatistiques();

                // Sauvegarder la bibliothèque dans un fichier
                string fichierSauvegarde = "bibliotheque.json";
                bibliothèque.SauvegarderBibliotheque(fichierSauvegarde);
                Console.WriteLine("Bibliothèque sauvegardée.");

                try
                {
                    // Charger la bibliothèque à partir du fichier
                    Library nouvelleBibliotheque = Library.ChargerBibliotheque(fichierSauvegarde);
                    Console.WriteLine("Bibliothèque chargée depuis le fichier.");

                    // Afficher les informations de la bibliothèque chargée
                    Console.WriteLine("Informations de la bibliothèque chargée :");
                    foreach (var media in bibliothèque.ObtenirTousLesMedias())
                    {
                        media.AfficherInfos();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors du chargement de la bibliothèque : {ex.Message}");
                }
            }
            catch (EmpruntInvalideException ex)
            {
                Console.WriteLine($"Erreur d'emprunt : {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }
        }
    }
}