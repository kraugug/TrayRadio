/*
 * Copyright(C) 2019 Michal Heczko
 * All rights reserved.
 *
 * This software may be modified and distributed under the terms
 * of the BSD license. See the LICENSE file for details.
 */

using System;
using System.IO;
using System.Text;

namespace TrayRadio
{
	public enum Id3v1TagGenre
	{
		Blues,
		ClassicRock,
		Country,
		Dance,
		Disco,
		Funk,
		Grunge,
		HipHop,
		Jazz,
		Metal,
		NewAge,
		Oldies,
		Other,
		Pop,
		RhythmAndBlues,
		Rap,
		Reggae,
		Rock,
		Techno,
		Industrial,
		Alternative,
		Ska,
		DeathMetal,
		Pranks,
		Soundtrack,
		EuroTechno,
		Ambient,
		TripHop,
		Vocal,
		JazzAndFunk,
		Fusion,
		Trance,
		Classical,
		Instrumental,
		Acid,
		House,
		Game,
		SoundClip,
		Gospel,
		Noise,
		AlternativeRock,
		Bass,
		Soul,
		Punk,
		Space,
		Meditative,
		InstrumentalPop,
		InstrumentalRock,
		Ethnic,
		Gothic,
		Darkwave,
		TechnoIndustrial,
		Electronic,
		PopFolk,
		Eurodance,
		Dream,
		SouthernRock,
		Comedy,
		Cult,
		Gangsta,
		Top40,
		ChristianRap,
		PopFunk,
		JungleMusic,
		NativeUS,
		Cabaret,
		NewWave,
		Psychedelic,
		Rave,
		Showtunes,
		Trailer,
		LoFi,
		Tribal,
		AcidPunk,
		AcidJazz,
		Polka,
		Retro,
		Musical,
		RocknRoll,
		HardRock
	}

	public class Id3v1Tag
	{
		#region Fields

		public static readonly int StringSize = 30;
		public static readonly int YearSize = 4;

		#endregion

		#region Properties

		public string Album { get; set; }

		public string Artist { get; set; }

		public string Comment { get; set; }

		public Id3v1TagGenre Genre { get; set; }

		public string Title { get; set; }

		public int Year;

		#endregion

		#region Methods

		public void WriteToStream(Stream stream)
		{
			if (stream == null)
				throw new NullReferenceException();
			// TAG
			byte[] buffer = new byte[] { 84, 65, 71 };
			stream.Write(buffer, 0, buffer.Length);
			// Title
			buffer = new byte[StringSize];
			Array.Clear(buffer, 0, buffer.Length);
			if (!string.IsNullOrEmpty(Title))
				Encoding.ASCII.GetBytes(Title).CopyTo(buffer, 0);
			stream.Write(buffer, 0, buffer.Length);
			// Artist
			Array.Clear(buffer, 0, buffer.Length);
			if (!string.IsNullOrEmpty(Artist))
				Encoding.ASCII.GetBytes(Artist).CopyTo(buffer, 0);
			stream.Write(buffer, 0, buffer.Length);
			// Album
			Array.Clear(buffer, 0, buffer.Length);
			if (!string.IsNullOrEmpty(Album))
				Encoding.ASCII.GetBytes(Album).CopyTo(buffer, 0);
			stream.Write(buffer, 0, buffer.Length);
			// Year
			Array.Clear(buffer, 0, buffer.Length);
			Encoding.ASCII.GetBytes(Year.ToString()).CopyTo(buffer, 0);
			stream.Write(buffer, 0, YearSize);
			// Comment
			Array.Clear(buffer, 0, buffer.Length);
			if (!string.IsNullOrEmpty(Comment))
				Encoding.ASCII.GetBytes(Comment).CopyTo(buffer, 0);
			stream.Write(buffer, 0, buffer.Length);
			// Genre
			stream.WriteByte((byte)Genre);
		}

		#endregion

		#region Constructor

		public Id3v1Tag()
		{
			Genre = Id3v1TagGenre.Other;
		}

		public Id3v1Tag(string album, string artist, string comment, Id3v1TagGenre genre, string title, int year)
		{
			Album = album != null ? album.Length > StringSize ? album.Substring(0, StringSize) : album : null;
			Artist = artist != null ? artist.Length > StringSize ? artist.Substring(0, StringSize) : artist : null;
			Comment = comment != null ? comment.Length > StringSize ? comment.Substring(0, StringSize) : comment : null;
			Genre = genre;
			Title = title != null ? title.Length > StringSize ? title.Substring(0, StringSize) : title : null;
			Year = year;
		}

		#endregion
	}
}
